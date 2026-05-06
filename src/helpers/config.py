from pydantic_settings import BaseSettings
from typing import List, Optional
from pathlib import Path

# Resolve .env from project root regardless of working directory
# src/helpers/config.py -> src/helpers -> src -> project_root
ENV_FILE = Path(__file__).resolve().parent.parent.parent / ".env"

class Settings(BaseSettings):

    APP_NAME: str
    APP_VERSION: str

    # secret Key
    GEMINI_API_KEY: str
    COHERE_API_KEY: str
    OPENAI_API_KEY: Optional[str] = None
    OPENAI_API_URL: Optional[str] = None
    
    # Langsmith
    LANGSMITH_TRACING: bool
    LANGSMITH_ENDPOINT: str
    LANGSMITH_API_KEY: str
    LANGSMITH_PROJECT: str

    # tavily
    TAVILY_API_KEY: str

    # youtube
    YOUTUBE_API_KEY: str

    # Chatbot
    ROUTER_LLM_TYPE: str
    ROUTER_LLM_MODEL: str

    SIMPLE_LLM_TYPE: str
    SIMPLE_LLM_MODEL: str

    MEDIUM_LLM_TYPE: str
    MEDIUM_LLM_MODEL: str

    ADVANCED_LLM_TYPE: str
    ADVANCED_LLM_MODEL: str

    # Embeddings
    EMBEDDING_MODEL_TYPE: str
    EMBEDDING_MODEL_NAME: str
    EMBEDDING_SIZE: int

    # Redis
    REDIS_HOST: str = "localhost"
    REDIS_PORT: int = 6379
    REDIS_PASSWORD: Optional[str] = None

    # Semantic Cache
    SIMILARITY_THRESHOLD: float = 0.85

    # Conversation Memory (Redis-backed)
    MEMORY_MAX_WINDOW: int = 20       # max raw messages kept per user
    MEMORY_SUMMARY_INTERVAL: int = 5  # recompute summary every N new messages

    # Optional cache tuning
    CACHE_SIMILARITY_THRESHOLD: Optional[float] = None  # overrides SIMILARITY_THRESHOLD if set
    CACHE_TTL_SECONDS: Optional[int] = None             # future: TTL per cache entry


    class Config:
        env_file = str(ENV_FILE)
        env_file_encoding = "utf-8"
        extra = "ignore"

    @property
    def effective_similarity_threshold(self) -> float:
        """Return CACHE_SIMILARITY_THRESHOLD if set, else fall back to SIMILARITY_THRESHOLD."""
        return self.CACHE_SIMILARITY_THRESHOLD if self.CACHE_SIMILARITY_THRESHOLD is not None else self.SIMILARITY_THRESHOLD

def get_settings() -> Settings:
    return Settings()
    