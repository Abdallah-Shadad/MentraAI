from dataclasses import dataclass
from .CacheEnums import DistanceMetric,VectorDataType,VectorIndexType,VectorDataType


@dataclass
class FlatVectorConfig:
    dim: int
    distance_metric: DistanceMetric = DistanceMetric.COSINE
    dtype: VectorDataType = VectorDataType.FLOAT32

    def to_redis_args(self) -> str:
        # Returns only the algo + attribute pairs (no leading "VECTOR" keyword).
        # FT.CREATE schema:  <field_name> VECTOR <algo> <n_attrs> <k> <v> ...
        # FLAT has 3 attr pairs → n_attrs = 6
        return (
            f"{VectorIndexType.FLAT.value} 6 "
            f"TYPE {self.dtype.value} "
            f"DIM {self.dim} "
            f"DISTANCE_METRIC {self.distance_metric.value}"
        )


@dataclass
class HNSWVectorConfig:
    dim: int
    m: int = 16
    ef_construction: int = 200
    ef_runtime: int = 10
    distance_metric: DistanceMetric = DistanceMetric.COSINE
    dtype: VectorDataType = VectorDataType.FLOAT32

    def to_redis_args(self) -> str:
        # Returns only the algo + attribute pairs (no leading "VECTOR" keyword).
        # FT.CREATE schema:  <field_name> VECTOR <algo> <n_attrs> <k> <v> ...
        # HNSW has 6 attr pairs → n_attrs = 12
        return (
            f"{VectorIndexType.HNSW.value} 12 "
            f"TYPE {self.dtype.value} "
            f"DIM {self.dim} "
            f"DISTANCE_METRIC {self.distance_metric.value} "
            f"M {self.m} "
            f"EF_CONSTRUCTION {self.ef_construction} "
            f"EF_RUNTIME {self.ef_runtime}"
        )