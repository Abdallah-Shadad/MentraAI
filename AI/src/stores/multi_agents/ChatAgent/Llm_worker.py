import logging
from typing import AsyncIterator

from helpers.config import get_settings
from stores.llm.LLMEnums import LLMEnums
from stores.llm.providers.GeminiProvider import GeminiProvider
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from .ChatEnums import ChatTypes

logger   = logging.getLogger(__name__)
settings = get_settings()


class LlmWorker:
    """
    Builds and caches the three tiered LLM providers at startup.

    Tier  │ Settings keys
    ──────┼──────────────────────────────────────
    simple   │ SIMPLE_LLM_TYPE  + SIMPLE_LLM_MODEL
    medium   │ MEDIUM_LLM_TYPE  + MEDIUM_LLM_MODEL
    advanced │ ADVANCED_LLM_TYPE + ADVANCED_LLM_MODEL

    All providers expose `.astream()` via the `stream()` method so
    ChatRouter always gets an `AsyncIterator[str]` regardless of tier.
    """

    def __init__(self):
        self._workers: dict = {
            ChatTypes.SIMPLE.value:   self._build(settings.SIMPLE_LLM_TYPE,   settings.SIMPLE_LLM_MODEL),
            ChatTypes.MEDIUM.value:   self._build(settings.MEDIUM_LLM_TYPE,   settings.MEDIUM_LLM_MODEL),
            ChatTypes.ADVANCED.value: self._build(settings.ADVANCED_LLM_TYPE, settings.ADVANCED_LLM_MODEL),
        }
        logger.info("LlmWorker initialised — tiers: %s", list(self._workers.keys()))

    # ─────────────────────────────────────────────
    # Internal builder
    # ─────────────────────────────────────────────

    def _build(self, llm_type: str, llm_model: str):
        """Instantiate and configure a provider from its type string."""
        if llm_type == LLMEnums.GEMINI.value:
            provider = GeminiProvider(api_key=settings.GEMINI_API_KEY)

        elif llm_type == LLMEnums.OPENAI.value:
            if not settings.OPENAI_API_KEY:
                raise RuntimeError(
                    "OPENAI_API_KEY is required in .env when using OPENAI as a chat LLM type."
                )
            provider = OpenAIProvider(
                api_key=settings.OPENAI_API_KEY,
                base_url=settings.OPENAI_API_URL or "",
            )

        else:
            raise ValueError(
                f"Unsupported LLM type '{llm_type}' for chat tiers. "
                f"Supported: GEMINI, OPENAI."
            )

        provider.set_generation_model(llm_model)
        logger.info("LlmWorker built — type=%s  model=%s", llm_type, llm_model)
        return provider

    # ─────────────────────────────────────────────
    # Public streaming API
    # ─────────────────────────────────────────────

    async def stream(self, chat_type: str, messages: list) -> AsyncIterator[str]:
        """
        Stream tokens from the provider matching the given chat_type.

        Args:
            chat_type: 'simple' | 'medium' | 'advanced'
            messages:  LangChain message objects (SystemMessage, HumanMessage …)

        Yields:
            str — one text chunk per token batch emitted by the model.
        """
        provider = self._workers.get(chat_type)
        if not provider:
            raise ValueError(
                f"Unknown chat_type: '{chat_type}'. "
                f"Expected one of: {list(self._workers.keys())}"
            )
        async for chunk in provider.stream(messages):
            yield chunk

    def get_provider(self, chat_type: str):
        """Return the raw provider for a given tier (used by RedisMemoryManager)."""
        return self._workers.get(chat_type)
