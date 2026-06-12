"""
Redis-backed Conversation Memory Manager
==========================================

Per-user, per-conversation memory stored entirely in Redis.
Survives FastAPI restarts.

Redis keys (scoped to BOTH user_id AND conversation_id):
────────────────────────────────────────────────────────────────
  chat:memory:{uid}:{conv_id}:messages   LIST    raw messages (JSON)
  chat:memory:{uid}:{conv_id}:summary    STRING  rolling summary text
  chat:memory:{uid}:{conv_id}:msg_count  STRING  total messages added
────────────────────────────────────────────────────────────────

Conversation isolation:
  • Same conversation_id  → history continues in the same bucket.
  • New  conversation_id  → a fresh empty bucket; no bleed between chats.
  • clear_conversation()  → wipes only one specific conversation bucket.

Behaviour:
  • RPUSH  — new messages are appended to the tail.
  • LTRIM  — after every push the list is trimmed to MAX_WINDOW.
  • Counter — incremented on every save; when counter % SUMMARY_INTERVAL == 0
              the LLM rewrites the summary from the current window.
  • get_context() returns:
        [SystemMessage(summary)]  ← if summary exists
      + [HumanMessage / AIMessage …]  ← recent window messages
"""

import json
import logging
import asyncio
from typing import Optional

import redis.asyncio as aioredis
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage

from helpers.config import get_settings

logger   = logging.getLogger(__name__)
settings = get_settings()

# ── Key builders ────────────────────────────────────────────────
# Keys are scoped to BOTH user_id and conversation_id so each
# conversation gets its own isolated history bucket.
_KEY_MESSAGES  = "chat:memory:{uid}:{conv_id}:messages"
_KEY_SUMMARY   = "chat:memory:{uid}:{conv_id}:summary"
_KEY_MSG_COUNT = "chat:memory:{uid}:{conv_id}:msg_count"

_SUMMARIZE_PROMPT = (
    "You are a conversation summarizer.\n"
    "Given the conversation below, write a concise summary (3-5 sentences) "
    "capturing the key topics, decisions, and context discussed.\n\n"
    "Conversation:\n{history}\n\n"
    "Summary:"
)


class RedisMemoryManager:
    """
    Async Redis-backed per-user conversation memory.

    Thread-safe for concurrent FastAPI requests because:
      • Each user has isolated Redis keys.
      • All operations are atomic Redis commands or short pipelines.
      • No in-process mutable state beyond the lazy Redis connection.
    """

    def __init__(self):
        self._redis: Optional[aioredis.Redis] = None
        self.max_window: int      = settings.MEMORY_MAX_WINDOW
        self.summary_interval: int = settings.MEMORY_SUMMARY_INTERVAL

    # ─────────────────────────────────────────────
    # Redis connection (lazy)
    # ─────────────────────────────────────────────

    async def _get_redis(self) -> aioredis.Redis:
        if self._redis is None:
            self._redis = aioredis.Redis(
                host=settings.REDIS_HOST,
                port=settings.REDIS_PORT,
                password=settings.REDIS_PASSWORD if settings.REDIS_PASSWORD else None,
                decode_responses=True,   # strings only — no bytes in this manager
            )
        return self._redis

    # ─────────────────────────────────────────────
    # Key helpers
    # ─────────────────────────────────────────────

    @staticmethod
    def _keys(user_id: str, conversation_id: str) -> tuple[str, str, str]:
        """Build the three Redis key names for a specific (user, conversation) pair."""
        uid     = user_id.strip()
        conv_id = conversation_id.strip()
        return (
            _KEY_MESSAGES.format(uid=uid, conv_id=conv_id),
            _KEY_SUMMARY.format(uid=uid, conv_id=conv_id),
            _KEY_MSG_COUNT.format(uid=uid, conv_id=conv_id),
        )

    # ─────────────────────────────────────────────
    # Serialisation helpers
    # ─────────────────────────────────────────────

    @staticmethod
    def _serialise(role: str, content: str) -> str:
        return json.dumps({"role": role, "content": content})

    @staticmethod
    def _deserialise(raw: str) -> dict:
        return json.loads(raw)

    # ─────────────────────────────────────────────
    # Public API
    # ─────────────────────────────────────────────

    async def get_context(
        self,
        user_id: str,
        conversation_id: str,
        system_prompt: str,
    ) -> list:
        """
        Build the message list for the LLM call.

        Args:
            user_id:         Unique user identifier.
            conversation_id: Scopes the memory bucket to a specific conversation.
            system_prompt:   The full system prompt (tier + mentor prefix already merged).

        Returns:
            [SystemMessage(system_prompt + "\n\n" + summary)]   ← merged
            + [HumanMessage | AIMessage …]                      ← recent window
        """
        redis = await self._get_redis()
        key_msgs, key_summary, _ = self._keys(user_id, conversation_id)

        # ── 1. Load summary + raw messages in parallel ──
        summary_raw, raw_msgs = await asyncio.gather(
            redis.get(key_summary),
            redis.lrange(key_msgs, 0, -1),
        )

        # ── 2. Build system message (prompt + summary) ──
        if summary_raw:
            system_content = (
                f"{system_prompt}\n\n"
                f"[Conversation summary so far]\n{summary_raw}"
            )
        else:
            system_content = system_prompt

        messages = [SystemMessage(content=system_content)]

        # ── 3. Deserialise raw messages into LangChain objects ──
        for raw in raw_msgs:
            msg = self._deserialise(raw)
            if msg["role"] == "human":
                messages.append(HumanMessage(content=msg["content"]))
            elif msg["role"] == "ai":
                messages.append(AIMessage(content=msg["content"]))

        logger.debug(
            "Memory GET — user=%s  conv=%s  summary=%s  window=%d messages",
            user_id, conversation_id, "yes" if summary_raw else "no", len(raw_msgs),
        )
        return messages

    async def add_messages(
        self,
        user_id: str,
        conversation_id: str,
        human_text: str,
        ai_text: str,
        summarizer_provider,          # LLMInterface — the simple-tier provider
    ) -> None:
        """
        Persist the latest human ↔ AI exchange and maybe recompute the summary.

        Args:
            user_id:              Unique user identifier.
            conversation_id:      Scopes the memory bucket to a specific conversation.
            human_text:           The user's query.
            ai_text:              The LLM's full response.
            summarizer_provider:  A ready-to-use LLM provider for summarization.
                                  (Runs in thread pool to avoid blocking the loop.)
        """
        redis = await self._get_redis()
        key_msgs, key_summary, key_count = self._keys(user_id, conversation_id)

        # ── 1. Push human + AI messages ──
        pipe = redis.pipeline()
        pipe.rpush(key_msgs, self._serialise("human", human_text))
        pipe.rpush(key_msgs, self._serialise("ai", ai_text))
        pipe.ltrim(key_msgs, -self.max_window, -1)   # keep last MAX_WINDOW only
        pipe.incr(key_count)
        results = await pipe.execute()

        # incr returns the new counter value (last result)
        new_count = int(results[-1])

        logger.debug(
            "Memory SET — user=%s  conv=%s  total_count=%d  window_limit=%d",
            user_id, conversation_id, new_count, self.max_window,
        )

        # ── 2. Recompute summary every SUMMARY_INTERVAL messages ──
        if new_count % self.summary_interval == 0:
            await self._recompute_summary(
                redis, key_msgs, key_summary, summarizer_provider
            )

    async def _recompute_summary(
        self,
        redis: aioredis.Redis,
        key_msgs: str,
        key_summary: str,
        summarizer_provider,
    ) -> None:
        """
        Ask the summarizer LLM to distil the current window into a short summary.

        The blocking LLM call runs in a thread pool so the event loop stays free.
        """
        raw_msgs = await redis.lrange(key_msgs, 0, -1)
        if not raw_msgs:
            return

        # Build a plain-text transcript for the summarizer
        lines = []
        for raw in raw_msgs:
            msg = self._deserialise(raw)
            prefix = "User" if msg["role"] == "human" else "Assistant"
            lines.append(f"{prefix}: {msg['content']}")
        history_text = "\n".join(lines)

        prompt = _SUMMARIZE_PROMPT.format(history=history_text)

        try:
            # invoke() is blocking — offload to thread pool
            summary = await asyncio.to_thread(summarizer_provider.invoke, prompt)
            if summary:
                await redis.set(key_summary, summary.strip())
                logger.info("Memory summary RECOMPUTED — key=%s", key_summary)
        except Exception as exc:
            logger.error("Summary recomputation failed: %s", exc)

    async def clear_conversation(self, user_id: str, conversation_id: str) -> None:
        """
        Delete all memory keys for a specific (user, conversation) pair.

        Only the three keys that belong to this exact conversation are removed;
        all other conversations for the same user remain intact.
        """
        redis = await self._get_redis()
        key_msgs, key_summary, key_count = self._keys(user_id, conversation_id)
        await redis.delete(key_msgs, key_summary, key_count)
        logger.info(
            "Memory CLEARED — user=%s  conv=%s", user_id, conversation_id
        )

    async def health_check(self) -> bool:
        """Verify Redis is reachable."""
        try:
            redis = await self._get_redis()
            await redis.ping()
            return True
        except Exception as exc:
            logger.error("RedisMemoryManager health check FAILED: %s", exc)
            return False
