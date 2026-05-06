from enum import Enum

class ChatTypes(Enum):
    SIMPLE = "simple"
    MEDIUM = "medium"
    ADVANCED = "advanced"

    GEMINI = "GEMINI"
    OPENAI = "OPENAI"
    COHERE = "COHERE"


class CacheEnums(Enum):
    CACHE_INDEX_NAME = "semantic_cache_idx"
    CACHE_KEY_PREFIX = "cache:"
    VECTOR_FIELD = "embedding"
    RESPONSE_FIELD = "response"
    QUERY_FIELD = "query"

    VECTOR_DISTANCE_METRIC_COSINE = "COSINE"
    VECTOR_DISTANCE_METRIC_EUCLIDEAN = "EUCLIDEAN"
    VECTOR_DISTANCE_METRIC_HAMMING = "HAMMING"
    VECTOR_DISTANCE_METRIC_INNER_PRODUCT = "INNER_PRODUCT"

    VECTOR_INDEX_FLAT = "FLAT"
    VECTOR_INDEX_HNSW = "HNSW"

    