import asyncio
import logging
from pydantic import BaseModel, Field
from typing import Literal, Annotated

from .ChatEnums import ChatTypes
from .prompts import ROUTER_SYSTEM_PROMPT
from stores.llm.LLMEnums import LLMEnums
from stores.llm.providers.GeminiProvider import GeminiProvider
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from helpers.config import get_settings

logger   = logging.getLogger(__name__)
settings = get_settings()


class RouterResponse(BaseModel):
    chat_type: Annotated[
        Literal["simple", "medium", "advanced"],
        Field(
            ...,
            description=(
                "Classify the user's query complexity to route it to the appropriate model power: "
                "'simple' for basic greetings or clear factual questions; "
                "'medium' for queries requiring multi-step logic or detailed explanations; "
                "'advanced' for highly complex, technical, or creative tasks that need deep reasoning."
            ),
        ),
    ]


class ClassifyAgent:
    """
    Lightweight LLM-based query classifier.

    Returns one of: 'simple' | 'medium' | 'advanced'

    The underlying `.invoke()` is synchronous (blocking network I/O).
    `classify_chat` offloads it to a thread pool via `asyncio.to_thread`
    so the FastAPI async event loop is never blocked.
    """

    def __init__(
        self,
        llm_type: str  = LLMEnums.GEMINI.value,
        llm_model: str = settings.ROUTER_LLM_MODEL,
    ):
        self.llm_type  = llm_type
        self.llm_model = llm_model

        from stores.llm.LLMManager import LLMManager
        from helpers.config import get_llm_config

        config = get_llm_config()
        self.llm_manager = LLMManager(config)
        self._chain = self.llm_manager.get_structured_chain("ClassifyAgent", RouterResponse)

    # ─────────────────────────────────────────────
    # Private — sync (runs in thread pool)
    # ─────────────────────────────────────────────

    def _classify_sync(self, query: str) -> str:
        """Blocking classify call — must NOT be called directly from async code."""
        prompt = ROUTER_SYSTEM_PROMPT + "\n\n" + query
        # Invoke the raw LangChain chain — returns RouterResponse directly.
        response: RouterResponse = self._chain.invoke(prompt)
        return response.chat_type

    # ─────────────────────────────────────────────
    # Public async API
    # ─────────────────────────────────────────────

    async def classify_chat(self, query: str) -> str:
        """
        Async entry point for query classification.

        Delegates the blocking LLM call to a thread pool so the
        event loop remains free for other concurrent requests.

        Returns:
            'simple' | 'medium' | 'advanced'
        """
        chat_type = await asyncio.to_thread(self._classify_sync, query)
        logger.info("ClassifyAgent → '%s' for query: %.60s", chat_type, query)
        return chat_type
