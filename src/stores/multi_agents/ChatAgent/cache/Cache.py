import asyncio
import hashlib
import logging
import numpy as np
import redis.asyncio as aioredis
from typing import Optional

from helpers.config import get_settings
from stores.llm.LLMEnums import DocumentTypeEnum
from .CacheEnums import CacheEnums, DistanceMetric, VectorDataType
from .VectorIndexConfig import FlatVectorConfig, HNSWVectorConfig
from ..embedder import Embedder


logger = logging.getLogger(__name__)
settings = get_settings()


# ─────────────────────────────────────────────────────────────────
# Utility Functions
# ─────────────────────────────────────────────────────────────────

def _query_hash(query: str) -> str:
    """Generate a stable MD5 hash for a normalized query string."""
    return hashlib.md5(query.strip().lower().encode()).hexdigest()


def _cosine_similarity(vec_a: list[float], vec_b: list[float]) -> float:
    """Compute cosine similarity between two float vectors."""
    a = np.array(vec_a)
    b = np.array(vec_b)
    dot  = np.dot(a, b)
    norm = np.linalg.norm(a) * np.linalg.norm(b)
    return float(dot / norm) if norm != 0 else 0.0


# ─────────────────────────────────────────────────────────────────
# Semantic Cache
# ─────────────────────────────────────────────────────────────────

class SemanticCache:
    """
    Async Redis-backed Semantic Cache using Redis Stack Vector Similarity
    Search (VSS) with a FLAT (brute-force) index.

    How it works:
        - Each cached entry is stored as a Redis Hash containing the
          original query text, the response text, and the query's
          float32 embedding.
        - On lookup, the incoming query is embedded and a KNN search
          finds the most similar stored entry. If the cosine similarity
          exceeds SIMILARITY_THRESHOLD, the cached response is returned
          immediately — skipping the LLM entirely.

    Index types:
        - FLAT  → brute-force KNN, accurate, good up to ~100k entries.
        - HNSW  → approximate KNN, faster at scale (millions of entries).
                  Swap FlatVectorConfig for HNSWVectorConfig to enable.
    """

    def __init__(self):
        self._redis: Optional[aioredis.Redis] = None
        self._index_ready: bool = False
        self.embedder: Embedder = Embedder()

    # ─────────────────────────────────────────────
    # Internal — Connection
    # ─────────────────────────────────────────────

    async def _get_redis(self) -> aioredis.Redis:
        """Return (and lazily initialize) the async Redis connection."""
        if self._redis is None:
            self._redis = aioredis.Redis(
                host=settings.REDIS_HOST,
                port=settings.REDIS_PORT,
                password=settings.REDIS_PASSWORD,
                decode_responses=False,
            )
        return self._redis

    # ─────────────────────────────────────────────
    # Index Management
    # ─────────────────────────────────────────────

    async def setup_index(self, force_recreate: bool = False) -> None:
        """
        Create the Redis vector search index if it does not already exist.

        Called once at application startup.  Safe to call multiple times —
        it is a no-op if the index is already present (unless force_recreate).

        Args:
            force_recreate: Drop and recreate the index even if it already
                            exists.  Use this once after deploying a schema
                            fix so the old broken index is replaced.
                            Existing cache Hash entries are deleted (DD flag)
                            so they can be re-populated with the correct schema.
        """
        redis = await self._get_redis()

        # ── 1. Check if the index already exists ──
        index_exists = False
        try:
            await redis.execute_command("FT.INFO", CacheEnums.CACHE_INDEX_NAME.value)
            index_exists = True
        except Exception:
            pass  # index doesn't exist yet — will create below

        if index_exists:
            if not force_recreate:
                logger.info("Semantic cache index already exists — skipping creation.")
                self._index_ready = True
                return
            # Drop the stale index + its documents so we start clean.
            try:
                await redis.execute_command(
                    "FT.DROPINDEX", CacheEnums.CACHE_INDEX_NAME.value, "DD"
                )
                logger.info("Stale cache index dropped (DD); rebuilding with correct schema.")
            except Exception as drop_exc:
                logger.warning("FT.DROPINDEX failed (non-fatal): %s", drop_exc)

        logger.info("Creating semantic cache index...")

        # ── 2. Build the vector index configuration ──

        # FLAT: brute-force KNN — accurate, suitable for up to ~100k entries.
        vector_config = FlatVectorConfig(
            dim=settings.EMBEDDING_SIZE,
            distance_metric=DistanceMetric.COSINE,
            dtype=VectorDataType.FLOAT32,
        )

        # HNSW: approximate KNN — uncomment for million-scale deployments.
        # vector_config = HNSWVectorConfig(
        #     dim=settings.EMBEDDING_SIZE,
        #     m=16,
        #     ef_construction=200,
        #     ef_runtime=10,
        #     distance_metric=DistanceMetric.COSINE,
        #     dtype=VectorDataType.FLOAT32,
        # )

        # ── 3. Issue FT.CREATE ──
        # Redis VSS schema for a VECTOR field:
        #   <field_name>  VECTOR  <algo>  <n_attrs*2>  TYPE <t>  DIM <d>  DISTANCE_METRIC <m>
        try:
            await redis.execute_command(
                "FT.CREATE", CacheEnums.CACHE_INDEX_NAME.value,
                "ON",     "HASH",
                "PREFIX", "1", CacheEnums.CACHE_KEY_PREFIX.value,
                "SCHEMA",
                CacheEnums.QUERY_FIELD.value,    "TEXT",
                CacheEnums.RESPONSE_FIELD.value, "TEXT",
                CacheEnums.VECTOR_FIELD.value,   "VECTOR",
                *vector_config.to_redis_args().split(),
            )
        except Exception as exc:
            logger.error(
                "FT.CREATE failed — semantic cache index NOT created: %s", exc
            )
            # _index_ready stays False; cache degrades gracefully
            return

        self._index_ready = True
        logger.info(
            "Semantic cache index '%s' created — dim=%d, metric=COSINE.",
            CacheEnums.CACHE_INDEX_NAME.value,
            settings.EMBEDDING_SIZE,
        )

    # ─────────────────────────────────────────────
    # Core Operations
    # ─────────────────────────────────────────────

    async def get(self, query: str) -> Optional[str]:
        """
        Look up a semantically similar query in the cache.

        Embeds the incoming query, performs a KNN=1 vector search, and
        returns the stored response if the cosine similarity is at or
        above settings.SIMILARITY_THRESHOLD.  Returns None on a miss.

        Args:
            query: The raw user query string.

        Returns:
            Cached response string on a hit, None on a miss.
        """
        if not self._index_ready:
            logger.warning("Cache index not ready — skipping lookup.")
            return None

        redis = await self._get_redis()

        # ── 1. Embed the incoming query (sync Cohere SDK → thread pool) ──
        embedding    = await asyncio.to_thread(
            self.embedder.client.embed_text,
            DocumentTypeEnum.QUERY.value,
            query.strip().replace("\n", " "),
        )
        if embedding is None:
            logger.warning("Cache GET — embedder returned None, skipping lookup.")
            return None
        query_vector = np.array(embedding[0], dtype=np.float32).tobytes()

        # ── 2. KNN vector search (Redis Stack VSS, DIALECT 2) ──
        try:
            result = await redis.execute_command(
                "FT.SEARCH", CacheEnums.CACHE_INDEX_NAME.value,
                f"*=>[KNN 1 @{CacheEnums.VECTOR_FIELD.value} $vec AS vec_score]",
                "PARAMS",  "2", "vec", query_vector,
                "RETURN",  "2", "vec_score", CacheEnums.RESPONSE_FIELD.value,
                "SORTBY",  "vec_score",
                "LIMIT",   "0", "1",
                "DIALECT", "2",
            )
        except Exception as exc:
            logger.error("FT.SEARCH failed: %s", exc)
            return None

        # result layout → [total_count, key, [field, value, ...], ...]
        if not result or result[0] == 0:
            logger.info("Cache MISS — no entries in index.")
            return None

        # ── 3. Parse the flat field-value list ──
        # Decode keys to str — Redis returns raw bytes (decode_responses=False).
        # Using uniform string keys eliminates any bytes/str mismatch on lookup.
        fields    = result[2]
        field_map = {fields[i].decode("utf-8"): fields[i + 1] for i in range(0, len(fields), 2)}

        raw_score    = field_map.get("vec_score")
        raw_response = field_map.get(CacheEnums.RESPONSE_FIELD.value)

        if raw_score is None or raw_response is None:
            logger.warning("Cache result is missing expected fields.")
            return None

        # ── 4. Evaluate similarity ──
        # Redis Stack COSINE distance: 0.0 = identical, 2.0 = opposite.
        # Convert to similarity:  similarity = 1 - distance
        distance   = float(raw_score)
        similarity = 1.0 - distance

        logger.info(
            "Cache lookup — similarity: %.4f | threshold: %.4f",
            similarity, settings.effective_similarity_threshold,
        )

        if similarity >= settings.effective_similarity_threshold:
            logger.info("Cache HIT (similarity=%.4f) — returning cached response.", similarity)
            return raw_response.decode("utf-8")

        logger.info("Cache MISS — similarity %.4f below threshold %.4f.", similarity, settings.effective_similarity_threshold)
        return None

    async def set(
        self,
        query: str,
        response: str,
        embedding: Optional[list[float]] = None,
    ) -> None:
        """
        Store a query–response pair in the semantic cache.

        If the caller already has the embedding (e.g. computed during
        the LLM call), pass it in via `embedding` to avoid a second
        API round-trip.

        Args:
            query:     The raw user query string.
            response:  The LLM response to cache.
            embedding: Optional pre-computed embedding for `query`.
        """
        if not self._index_ready:
            logger.warning("Cache SET skipped — index not ready.")
            return

        redis = await self._get_redis()

        # Reuse caller-supplied embedding or compute a fresh one (thread pool).
        # MUST use the same input_type as get() — both embed the user's query,
        # so search_query is correct.  Using search_document here would produce
        # a different vector space and every lookup would be a MISS.
        if embedding is None:
            raw = await asyncio.to_thread(
                self.embedder.client.embed_text,
                DocumentTypeEnum.QUERY.value,   # ← must match get()
                query.strip().replace("\n", " "),
            )
            if raw is None:
                logger.warning("Cache SET skipped — embedder returned None.")
                return
            embedding = raw[0]

        query_vector = np.array(embedding, dtype=np.float32).tobytes()
        cache_key    = f"{CacheEnums.CACHE_KEY_PREFIX.value}{_query_hash(query)}"

        await redis.hset(
            cache_key,
            mapping={
                CacheEnums.QUERY_FIELD.value:    query,
                CacheEnums.RESPONSE_FIELD.value: response,
                CacheEnums.VECTOR_FIELD.value:   query_vector,
            },
        )

        logger.info("Cache SET — key: %s", cache_key)

    async def invalidate(self, query: str) -> bool:
        """
        Remove a specific cached entry by its query string.

        Args:
            query: The exact query string that was originally cached.

        Returns:
            True if the entry existed and was deleted, False otherwise.
        """
        redis     = await self._get_redis()
        cache_key = f"{CacheEnums.CACHE_KEY_PREFIX.value}{_query_hash(query)}"
        deleted   = await redis.delete(cache_key)

        if deleted:
            logger.info("Cache INVALIDATED — key: %s", cache_key)
        else:
            logger.warning("Cache INVALIDATE — key not found: %s", cache_key)

        return bool(deleted)

    async def flush_all(self) -> int:
        """
        Delete all entries from the semantic cache.

        Scans and removes every Redis key that matches the cache prefix.
        Returns the total number of deleted keys.

        ⚠️  Use with caution in production — this is irreversible.
        """
        redis         = await self._get_redis()
        pattern       = f"{CacheEnums.CACHE_KEY_PREFIX.value}*"
        deleted_count = 0

        async for key in redis.scan_iter(pattern):
            await redis.delete(key)
            deleted_count += 1

        logger.warning("Cache FLUSHED — %d entries removed.", deleted_count)
        return deleted_count

    async def health_check(self) -> bool:
        """
        Verify that Redis is reachable and the vector index is active.

        Returns:
            True if both the Redis ping and FT.INFO succeed, False otherwise.
        """
        try:
            redis = await self._get_redis()
            await redis.ping()
            await redis.execute_command("FT.INFO", CacheEnums.CACHE_INDEX_NAME.value)
            logger.info("Cache health check PASSED.")
            return True
        except Exception as exc:
            logger.error("Cache health check FAILED: %s", exc)
            return False


# ─────────────────────────────────────────────────────────────────
# Singleton — import `semantic_cache` anywhere in the project
# ─────────────────────────────────────────────────────────────────

semantic_cache = SemanticCache()
