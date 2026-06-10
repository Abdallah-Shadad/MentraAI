import logging
from typing import List, Optional, Any, AsyncIterator

from langchain_groq import ChatGroq
from langchain_core.tools import BaseTool

from ..LLMInterface import LLMInterface
from ..LLMEnums import DocumentType, OpenAIEnums  # Reusing OpenAI Enums for simplicity as roles are similar

class GroqProvider(LLMInterface):

    def __init__(self, api_key: str,
                 max_output_tokens: int = 8000,
                 temperature: float = 0.1,
                 ):
        # Base Configuration
        self.api_key = api_key
        self.max_output_tokens = max_output_tokens
        self.temperature = temperature

        # Name of model
        self.generation_model_id = None

        self.enums = OpenAIEnums # Groq uses similar role structures

        self.client = None
        self.logger = logging.getLogger("uvicorn.error")

    def set_generation_model(self, model_id: str):
        """Create Client with Specific model_id"""
        self.generation_model_id = model_id
        self.client = ChatGroq(
            model=self.generation_model_id,
            api_key=self.api_key,
            temperature=self.temperature,
            max_tokens=self.max_output_tokens,
        )

    def set_embedding_model(self, model_id: str, embedding_size: int):
        # Groq doesn't natively provide an embedding model in the same way via ChatGroq,
        # but we must implement the interface.
        self.logger.warning("GroqProvider does not support embeddings natively yet.")
        pass

    def invoke(self, prompt: str, chat_history: list = [], max_output_tokens: int = None, temperature: float = None):
        self.logger.info(f"Generating text using #GroqProvider: {self.generation_model_id}")

        if not self.client:
            self.logger.error("Groq Client is not initialized")
            raise Exception("Groq client is not initialized")

        if not self.generation_model_id:
            self.logger.error("Generation model is not set")
            raise Exception("Generation model is not set")

        chat_history.append(
            self.construct_prompt(prompt=prompt, role=self.enums.USER.value)
        )

        try:
            response = self.client.invoke(chat_history)
            return response.content
        except Exception as e:
            self.logger.error(f"[GroqProvider] invoke failed: {e}")
            return None

    async def stream(self, messages: list) -> AsyncIterator[str]:
        if not self.client:
            raise Exception("Groq client is not initialized")
        if not self.generation_model_id:
            raise Exception("Generation model is not set")

        try:
            async for chunk in self.client.astream(messages):
                content = chunk.content
                if isinstance(content, list):
                    parts = []
                    for part in content:
                        if isinstance(part, str):
                            parts.append(part)
                        elif isinstance(part, dict) and part.get("type") == "text":
                            parts.append(part.get("text", ""))
                    content = "".join(parts)

                if content:
                    yield content
        except Exception as e:
            self.logger.error(f"[GroqProvider] stream failed: {e}")
            raise e

    def embed_text(self, document_type: str, document_content: str = None):
        self.logger.warning("GroqProvider does not support embeddings natively yet.")
        return None

    def construct_prompt(self, prompt: str, role: str):
        # Groq uses standard OpenAI-like message format
        return {"role": role, "content": prompt}

    def bind_tools(self, tools: List[BaseTool]):
        if not self.client:
            self.logger.error("Groq Client is not initialized")
            raise Exception("Groq client is not initialized")

        self.client = self.client.bind_tools(tools)
        self.logger.info(f"[GroqProvider] Bound {len(tools)} tools to model.")

    def with_structured_output(self, schema: Any, **kwargs):
        """
        Bind a Pydantic output schema to the LLM.
        """
        if not self.client:
            raise RuntimeError("Groq client is not initialized. Call set_generation_model() first.")

        self.client = self.client.with_structured_output(schema, **kwargs)
        self.logger.info(f"[GroqProvider] Bound structured output schema.")
        return self
