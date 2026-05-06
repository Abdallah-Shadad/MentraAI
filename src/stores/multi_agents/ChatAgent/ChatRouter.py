"""
ChatRouter — Async Streaming Router with Semantic Cache & Redis Memory
=======================================================================

Entry-point for all chat requests.  Exposes two public methods:

  resolve(user_id, query) → (cache_hit: bool, stream: AsyncGenerator)
      Checks the semantic cache eagerly, BEFORE yielding any tokens.
      Returns the cache status immediately so callers can set response
      headers (e.g. X-Cache: HIT/MISS) before streaming begins.

  chat(user_id, query) → AsyncGenerator[str, None]
      Thin wrapper around resolve() — kept for backward compatibility.

FastAPI usage (preferred — exposes X-Cache header):
    is_hit, stream = await router.resolve(user_id, query)
    return StreamingResponse(
        stream,
        headers={"X-Cache": "HIT" if is_hit else "MISS"},
        media_type="text/plain",
    )
"""

import asyncio
import logging
from typing import AsyncGenerator

from langchain_core.messages import HumanMessage

from .ClassifyAgent import ClassifyAgent
from .Llm_worker import LlmWorker
from .cache.Cache import semantic_cache
from .cache.RedisMemory import RedisMemoryManager
from .ChatEnums import ChatTypes
from .prompts import SIMPLE_CHAT_PROMPT, MEDIUM_CHAT_PROMPT, ADVANCED_CHAT_PROMPT

logger = logging.getLogger(__name__)

# ── Tier → system prompt mapping ────────────────────────────────
_TIER_PROMPTS: dict[str, str] = {
    ChatTypes.SIMPLE.value:   SIMPLE_CHAT_PROMPT,
    ChatTypes.MEDIUM.value:   MEDIUM_CHAT_PROMPT,
    ChatTypes.ADVANCED.value: ADVANCED_CHAT_PROMPT,
}

# ── Cache replay chunk size (characters per yielded chunk) ──────
_REPLAY_CHUNK_SIZE = 32


class ChatRouter:
    """
    Async streaming chat router.

    Instantiate once at application startup (singleton).
    Call `await setup()` after construction to initialise the Redis
    vector index for the semantic cache.

    All state is either:
      • Immutable after __init__ (providers, agents)
      • Isolated per user_id in Redis (memory)
      • Stateless per request (cache lookups, classify)

    This makes ChatRouter safe for concurrent FastAPI requests.
    """

    def __init__(self):
        self.classifier  = ClassifyAgent()
        self.llm_worker  = LlmWorker()
        self.memory      = RedisMemoryManager()
        logger.info("ChatRouter initialised.")

    # ─────────────────────────────────────────────
    # Startup
    # ─────────────────────────────────────────────

    async def setup(self) -> None:
        """
        Run once at app startup.

        Reuses the existing Redis vector index if it is already present —
        cached data survives server restarts.  Only creates the index from
        scratch when it does not yet exist.

        Pass force_recreate=True manually (once) only if the index schema
        needs to be rebuilt after a code change.
        """
        await semantic_cache.setup_index(force_recreate=False)
        logger.info("ChatRouter setup complete — semantic cache index ready.")

    # ─────────────────────────────────────────────
    # Public API
    # ─────────────────────────────────────────────

    async def resolve(
        self,
        user_id: str,
        query: str,
    ) -> tuple[bool, AsyncGenerator[str, None]]:
        """
        Eagerly check the semantic cache, then return the stream.

        The cache lookup is awaited **before** any token is yielded, so
        the caller knows the cache status immediately and can include it
        in HTTP response headers before streaming begins.

        Args:
            user_id: Unique identifier for the user.
            query:   The raw user message.

        Returns:
            (cache_hit, stream) where:
              cache_hit – True if the response is served from Redis cache.
              stream    – AsyncGenerator that yields text chunks.

        FastAPI usage:
            is_hit, stream = await router.resolve(uid, q)
            return StreamingResponse(
                stream,
                headers={"X-Cache": "HIT" if is_hit else "MISS"},
                media_type="text/plain",
            )
        """
        cached = await semantic_cache.get(query)
        if cached:
            logger.info("ChatRouter — cache HIT for user=%s", user_id)
            return True, self._replay_stream(cached)

        logger.info("ChatRouter — cache MISS for user=%s", user_id)
        return False, self._live_stream(user_id, query)

    async def chat(
        self,
        user_id: str,
        query: str,
    ) -> AsyncGenerator[str, None]:
        """
        Convenience wrapper around resolve() — yields tokens directly.

        Prefer `resolve()` in FastAPI endpoints so you can set the
        X-Cache response header before streaming begins.
        """
        _, stream = await self.resolve(user_id, query)
        async for chunk in stream:
            yield chunk

    # ─────────────────────────────────────────────
    # Internal stream generators
    # ─────────────────────────────────────────────

    async def _live_stream(
        self,
        user_id: str,
        query: str,
    ) -> AsyncGenerator[str, None]:
        """
        Classify → build context → stream LLM → persist.

        Called only on a cache miss.  Runs the full pipeline and
        fires-and-forgets cache + memory persistence after streaming.
        """
        # ── Step 1: Classify query complexity ───────────────────
        chat_type = await self.classifier.classify_chat(query)
        logger.info(
            "ChatRouter — classified as '%s' for user=%s", chat_type, user_id
        )

        # ── Step 2: Build context (system prompt + memory + query) ──
        system_prompt = _TIER_PROMPTS.get(chat_type, SIMPLE_CHAT_PROMPT)
        messages      = await self.memory.get_context(user_id, system_prompt)
        messages.append(HumanMessage(content=query))

        # ── Step 3: Stream LLM response ─────────────────────────
        chunks: list[str] = []
        try:
            async for chunk in self.llm_worker.stream(chat_type, messages):
                chunks.append(chunk)
                yield chunk
        except Exception as exc:
            logger.error(
                "ChatRouter — LLM stream error (user=%s tier=%s): %s",
                user_id, chat_type, exc,
            )
            yield "\n[Error: failed to generate response. Please try again.]"
            return

        full_response = "".join(chunks)

        # ── Step 4: Persist (fire-and-forget, non-blocking) ─────
        # Skip if nothing was generated (e.g. all chunks were empty).
        if not full_response.strip():
            logger.warning("ChatRouter — empty response, skipping cache/memory persist.")
            return

        logger.info(
            "ChatRouter — scheduling persist for user=%s tier=%s (%d chars)",
            user_id, chat_type, len(full_response),
        )
        asyncio.create_task(
            self._persist(user_id, query, full_response, chat_type)
        )

    # ─────────────────────────────────────────────
    # Internal helpers
    # ─────────────────────────────────────────────

    async def _persist(
        self,
        user_id: str,
        query: str,
        full_response: str,
        chat_type: str,
    ) -> None:
        """
        Save the completed exchange to cache + memory.

        Runs as an asyncio Task (fire-and-forget) so the HTTP response
        is not delayed by post-processing I/O.
        """
        summarizer = self.llm_worker.get_provider(ChatTypes.SIMPLE.value)
        try:
            await asyncio.gather(
                semantic_cache.set(query, full_response),
                self.memory.add_messages(
                    user_id, query, full_response, summarizer
                ),
            )
            logger.info(
                "ChatRouter — persisted exchange for user=%s tier=%s",
                user_id, chat_type,
            )
        except Exception as exc:
            logger.error("ChatRouter — persist error (user=%s): %s", user_id, exc)

    @staticmethod
    async def _replay_stream(
        text: str,
        chunk_size: int = _REPLAY_CHUNK_SIZE,
    ) -> AsyncGenerator[str, None]:
        """
        Replay a cached full response as a token stream.

        Yields fixed-size chunks and yields control to the event loop
        between each chunk so other requests aren't starved.
        """
        for i in range(0, len(text), chunk_size):
            yield text[i : i + chunk_size]
            await asyncio.sleep(0)   # yield event loop between chunks

    # ─────────────────────────────────────────────
    # Maintenance
    # ─────────────────────────────────────────────

    async def clear_user_memory(self, user_id: str) -> None:
        """Wipe all conversation history for a user (e.g. on session reset)."""
        await self.memory.clear(user_id)
        logger.info("ChatRouter — memory cleared for user=%s", user_id)

    async def health_check(self) -> dict:
        """Return health status of cache and memory subsystems."""
        cache_ok, memory_ok = await asyncio.gather(
            semantic_cache.health_check(),
            self.memory.health_check(),
        )
        return {
            "semantic_cache": "ok" if cache_ok  else "error",
            "redis_memory":   "ok" if memory_ok else "error",
        }