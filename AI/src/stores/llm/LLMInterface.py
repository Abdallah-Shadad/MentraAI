from abc import ABC, abstractmethod
from typing import List, AsyncIterator
from langchain_core.tools import BaseTool


class LLMInterface(ABC):

    @abstractmethod
    def set_generation_model(self, model_id: str):
        pass

    @abstractmethod
    def set_embedding_model(self, model_id: str, embedding_size: int):
        pass

    @abstractmethod
    def invoke(self, prompt: str, chat_history: list = [], max_output_tokens: int = None, temperature: float = None):
        pass

    @abstractmethod
    async def stream(self, messages: list) -> AsyncIterator[str]:
        """
        Async token-by-token streaming.

        Args:
            messages: List of LangChain message dicts / message objects.

        Yields:
            str — one text chunk per token batch from the model.
        """
        pass

    @abstractmethod
    def embed_text(self, document_type: str, document_content: str = None):
        pass

    @abstractmethod
    def construct_prompt(self, prompt: str, role: str):
        pass

    @abstractmethod
    def bind_tools(self, tools: List[BaseTool]):
        pass
