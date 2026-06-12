"""
ChatRouter — Async Streaming Router with Semantic Cache & Redis Memory
=======================================================================

Entry-point for all chat requests.  Exposes two public methods:

  resolve(ctx, query) → (cache_hit: bool, stream: AsyncGenerator)
      ctx   – MentorContext carrying user_id, conversation_id, and optional
              learner state (career_track, stage, lesson_id, quiz_*).
      Checks the semantic cache eagerly, BEFORE yielding any tokens.
      Returns the cache status immediately so callers can set response
      headers (e.g. X-Cache: HIT/MISS) before streaming begins.

  chat(ctx, query) → AsyncGenerator[str, None]
      Thin wrapper around resolve() — kept for backward compatibility.

Cache key strategy (Context-Enriched Semantic Query):

    The raw query is enriched with available learner context tags before
    being embedded and stored.  This means semantically similar queries
    asked at different learning stages or in different career tracks produce
    DIFFERENT cache entries, so:

      • "what is looping? [track:backend | stage:python_basics]" → Python answer
      • "what is looping? [track:frontend | stage:javascript_basics]" → JS answer

    Two learners at the same stage asking the same question still get a
    cache HIT (the enriched strings are semantically identical → high
    cosine similarity).

    quiz_score is intentionally excluded from the enriched query — it
    affects the tone/framing of the answer (handled by the system prompt
    mentor prefix), NOT the factual content.  Caching by quiz score would
    fragment the cache and reduce hit rate with no correctness benefit.

FastAPI usage (preferred — exposes X-Cache header):
    is_hit, stream = await router.resolve(ctx, query)
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
from .MentorContext import MentorContext, build_mentor_prefix
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


def _build_cache_query(query: str, ctx: MentorContext) -> str:
    """
    Build a context-enriched query string for semantic cache lookup & storage.

    The raw query is extended with available learner context tags so the
    Cohere embedder produces DIFFERENT vectors for different learning contexts.

    Examples
    --------
    No context (new user):
        "what is looping?"

    With context:
        "what is looping? [track:backend | stage:python_basics | lesson:loops_intro]"
        "what is looping? [track:frontend | stage:javascript_basics | lesson:loops_intro]"

    The two context-enriched strings above will have low cosine similarity
    despite sharing the same raw question — they will NOT hit each other's
    cache entries.

    Note: quiz_score is deliberately excluded.  It only affects the tone
    of the answer (handled by the system prompt), not the factual content.
    Caching by score would fragment the cache without correctness benefit.
    """
    tags: list[str] = []
    if ctx.career_track:
        tags.append(f"track:{ctx.career_track}")
    if ctx.stage:
        tags.append(f"stage:{ctx.stage}")
    if ctx.lesson_id:
        tags.append(f"lesson:{ctx.lesson_id}")

    if not tags:
        return query.strip()   # new user — no context, plain query

    return f"{query.strip()} [{' | '.join(tags)}]"


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
        ctx: MentorContext,
        query: str,
    ) -> tuple[bool, AsyncGenerator[str, None]]:
        """
        Eagerly check the semantic cache, then return the stream.

        Cache is checked BEFORE any token is yielded so the caller can include
        the X-Cache header in the HTTP response before streaming begins.

        Args:
            ctx:   MentorContext holding user_id, conversation_id, and optional
                   learner state fields.
            query: The raw user message.

        Returns:
            (cache_hit, stream) where:
              cache_hit – True if the response is served from Redis cache.
              stream    – AsyncGenerator that yields text chunks.

        FastAPI usage:
            is_hit, stream = await router.resolve(ctx, q)
            return StreamingResponse(
                stream,
                headers={"X-Cache": "HIT" if is_hit else "MISS"},
                media_type="text/plain",
            )
        """
        # ── Step 1: Classify first (needed for routing tier) ────────
        chat_type = await self.classifier.classify_chat(query)

        # ── Step 2: Build context-enriched query for semantic cache ──
        # career_track + stage + lesson_id change the CONTENT of the answer.
        # quiz_score only changes the tone — excluded from cache key.
        enriched_query = _build_cache_query(query, ctx)

        # ── Step 3: Check semantic cache with enriched query ─────────
        cached = await semantic_cache.get(enriched_query)
        if cached:
            logger.info(
                "ChatRouter — cache HIT  user=%s  conv=%s  tier=%s  enriched_q='%.60s'",
                ctx.user_id, ctx.conversation_id, chat_type, enriched_query,
            )
            return True, self._replay_stream(cached)

        logger.info(
            "ChatRouter — cache MISS user=%s  conv=%s  tier=%s  enriched_q='%.60s'",
            ctx.user_id, ctx.conversation_id, chat_type, enriched_query,
        )
        return False, self._live_stream(ctx, query, enriched_query, chat_type)

    async def chat(
        self,
        ctx: MentorContext,
        query: str,
    ) -> AsyncGenerator[str, None]:
        """
        Convenience wrapper around resolve() — yields tokens directly.

        Prefer `resolve()` in FastAPI endpoints so you can set the
        X-Cache response header before streaming begins.
        """
        _, stream = await self.resolve(ctx, query)
        async for chunk in stream:
            yield chunk

    # ─────────────────────────────────────────────
    # Internal stream generators
    # ─────────────────────────────────────────────

    async def _live_stream(
        self,
        ctx: MentorContext,
        query: str,
        enriched_query: str,         # pre-built in resolve() — used for cache.set()
        chat_type: str,              # already classified in resolve()
    ) -> AsyncGenerator[str, None]:
        """
        Build context → stream LLM → persist.

        `chat_type` is passed in from resolve() to avoid classifying twice
        (resolve() already ran the classifier to compute the cache key).

        Called only on a cache miss.  Runs the full pipeline and
        fires-and-forgets cache + memory persistence after streaming.
        """
        user_id         = ctx.user_id
        conversation_id = ctx.conversation_id

        logger.info(
            "ChatRouter — classified as '%s' for user=%s  conv=%s",
            chat_type, user_id, conversation_id,
        )

        # ── Step 1: Build system prompt = mentor prefix + tier prompt ───
        tier_prompt   = _TIER_PROMPTS.get(chat_type, SIMPLE_CHAT_PROMPT)
        mentor_prefix = build_mentor_prefix(ctx)          # "" for new users
        system_prompt = mentor_prefix + tier_prompt

        # ── Step 2: Load conversation memory ───────────────────────────
        messages = await self.memory.get_context(
            user_id, conversation_id, system_prompt
        )
        messages.append(HumanMessage(content=query))

        # ── Step 3: Stream LLM response ──────────────────────────────
        chunks: list[str] = []
        try:
            async for chunk in self.llm_worker.stream(chat_type, messages):
                chunks.append(chunk)
                yield chunk
        except Exception as exc:
            logger.error(
                "ChatRouter — LLM stream error (user=%s conv=%s tier=%s): %s",
                user_id, conversation_id, chat_type, exc,
            )
            if not chunks:
                raise exc
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
            self._persist(
                ctx.user_id,
                ctx.conversation_id,
                query,
                enriched_query,          # context-enriched — used as cache key
                full_response,
                chat_type,
            )
        )

    # ─────────────────────────────────────────────
    # Internal helpers
    # ─────────────────────────────────────────────

    async def _persist(
        self,
        user_id: str,
        conversation_id: str,
        query: str,
        enriched_query: str,
        full_response: str,
        chat_type: str,
    ) -> None:
        """
        Save the completed exchange to semantic cache + Redis memory.

        Cache: stored with the ENRICHED query so future lookups with the
        same learner context (career_track + stage + lesson_id) get a HIT.
        Memory: stored with the RAW query so conversation history is human-readable.

        Runs as an asyncio Task (fire-and-forget) so the HTTP response
        is not delayed by post-processing I/O.
        """
        summarizer = self.llm_worker.get_provider(ChatTypes.SIMPLE.value)
        try:
            await asyncio.gather(
                semantic_cache.set(enriched_query, full_response),  # enriched key
                self.memory.add_messages(
                    user_id, conversation_id, query, full_response, summarizer
                ),
            )
            logger.info(
                "ChatRouter — persisted  user=%s  conv=%s  tier=%s  cache_q='%.60s'",
                user_id, conversation_id, chat_type, enriched_query,
            )
        except Exception as exc:
            logger.error(
                "ChatRouter — persist error (user=%s  conv=%s): %s",
                user_id, conversation_id, exc,
            )

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

    async def clear_conversation(
        self,
        user_id: str,
        conversation_id: str,
    ) -> None:
        """Wipe conversation history for a specific (user, conversation) pair."""
        await self.memory.clear_conversation(user_id, conversation_id)
        logger.info(
            "ChatRouter — memory cleared for user=%s  conv=%s",
            user_id, conversation_id,
        )

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