from enum import Enum

class CacheEnums(Enum):
    CACHE_INDEX_NAME = "semantic_cache_idx"
    CACHE_KEY_PREFIX = "cache:"
    VECTOR_FIELD = "embedding"
    RESPONSE_FIELD = "response"
    QUERY_FIELD = "query"


class VectorIndexType(str, Enum):
    FLAT = "FLAT"
    HNSW = "HNSW"


class DistanceMetric(str, Enum):
    COSINE = "COSINE"
    L2 = "L2"
    IP = "IP"


class VectorDataType(str, Enum):
    FLOAT32 = "FLOAT32"