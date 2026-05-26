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
    
    # Agent-Specific Gemini Keys (each gets its own quota bucket)
    GEMINI_API_KEY_SUPERVISOR: str = ""
    GEMINI_API_KEY_PROFILE: str = ""
    GEMINI_API_KEY_CURRICULUM: str = ""
    GEMINI_API_KEY_RESOURCE: str = ""
    GEMINI_API_KEY_ADAPTATION: str = ""

    # Groq fallback keys (2 keys — first hits limit → switches to second)
    GROQ_API_KEY_1: str = ""
    GROQ_API_KEY_2: str = ""
    
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

def get_llm_config() -> dict:
    """
    Returns the centralized configuration for LLMs and multi-agent load balancing.
    Every agent has:
      - A primary Gemini model with its own dedicated API key
      - A Groq fallback that activates automatically on rate-limit errors (429)
    """
    import os
    settings = None
    try:
        settings = get_settings()
    except Exception:
        pass # Ignore if .env is missing for now

    default_gemini_key = settings.GEMINI_API_KEY if settings else os.getenv("GEMINI_API_KEY", "")
    groq_key_1 = getattr(settings, "GROQ_API_KEY_1", "") or os.getenv("GROQ_API_KEY_1", "") if settings else ""
    groq_key_2 = getattr(settings, "GROQ_API_KEY_2", "") or os.getenv("GROQ_API_KEY_2", "") if settings else ""

    # Retrieve agent-specific keys, falling back to the default Gemini key
    def gemini_key(attr):
        return (getattr(settings, attr, "") or default_gemini_key) if settings else default_gemini_key

    # Build Groq fallback list — each key is a separate fallback in sequence
    # LangChain will try fallback[0] first, then fallback[1] if that also fails
    def groq_fallback(key, model="llama-3.3-70b-versatile"):
        return {"provider": "groq", "model": model, "api_key": key, "temperature": 0.1}

    fallbacks = []
    if groq_key_1:
        fallbacks.append(groq_fallback(groq_key_1))
    if groq_key_2:
        fallbacks.append(groq_fallback(groq_key_2))

    return {
        # Default Provider (used by Supervisor directly in routes/roadmap.py)
        "api_key": gemini_key("GEMINI_API_KEY_SUPERVISOR"),
        "base_url": "https://8ae4-34-187-223-8.ngrok-free.app/v1/",
        "max_output_tokens": 100000,
        "temperature": 0.1,
        "model": "qwen3:8b",
        "provider": "openai",

        # ── Agent-Specific LLM Configs with Groq Fallback ──────────────────
        "agent_llm_configs": {
            "ProfileAnalyzer": {
                "primary": {
                    "provider": "gemini",
                    "model": "gemini-2.5-flash-lite",
                    "api_key": gemini_key("GEMINI_API_KEY_PROFILE"),
                    "temperature": 0.1
                },
                "fallbacks": fallbacks
            },
            "CurriculumGenerator": {
                "primary": {
                    "provider": "gemini",
                    "model": "gemini-2.5-flash",
                    "api_key": gemini_key("GEMINI_API_KEY_CURRICULUM"),
                    "temperature": 0.1
                },
                "fallbacks": fallbacks
            },
            "ResourceCurator": {
                "primary": {
                    "provider": "gemini",
                    "model": "gemini-2.5-flash-lite",
                    "api_key": gemini_key("GEMINI_API_KEY_RESOURCE"),
                    "temperature": 0.1
                },
                "fallbacks": fallbacks
            },
            "AdaptationEngine": {
                "primary": {
                    "provider": "gemini",
                    "model": "gemini-2.5-flash",
                    "api_key": gemini_key("GEMINI_API_KEY_ADAPTATION"),
                    "temperature": 0.1
                },
                "fallbacks": fallbacks
            }
        }
    }
