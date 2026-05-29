import logging
from typing import List, Optional, Any, AsyncIterator

from langchain_google_genai import ChatGoogleGenerativeAI, GoogleGenerativeAIEmbeddings
from langchain_core.tools import BaseTool

from ..LLMInterface import LLMInterface
from ..LLMEnums import DocumentType, GeminiEnums


class GeminiProvider(LLMInterface):

    def __init__(self, api_key: str,
                 max_output_tokens: int = 100000,
                 temperature: float = 0.1,
                 ):
        # Base Configuration
        self.api_key = api_key
        self.max_output_tokens = max_output_tokens
        self.temperature = temperature

        # Name of model
        self.generation_model_id = None

        # Embedding model
        self.embedding_model_id = None
        self.embedding_size = None

        self.enums = GeminiEnums

        self.client = None
        self.embedding_model = None
        self.logger = logging.getLogger("uvicorn.error")

    def set_generation_model(self, model_id: str):
        """Create Client with Specific model_id"""
        self.generation_model_id = model_id
        self.client = ChatGoogleGenerativeAI(
            model=self.generation_model_id,
            google_api_key=self.api_key,
            max_output_tokens=self.max_output_tokens,
            temperature=self.temperature,
            max_retries=0,  # Disable SDK auto-retry so LangChain fallbacks trigger instantly on 429
        )

    def set_embedding_model(self, model_id: str, embedding_size: int):
        self.embedding_model_id = model_id
        self.embedding_size = embedding_size
        self.embedding_model = GoogleGenerativeAIEmbeddings(
            model=self.embedding_model_id,
            google_api_key=self.api_key,
        )

    def invoke(self, prompt: str, chat_history: list = [], max_output_tokens: int = None, temperature: float = None):
        self.logger.info(f"Generating text using #GeminiProvider: {self.generation_model_id}")

        if not self.client:
            self.logger.error("Gemini Client is not initialized")
            raise Exception("Gemini client is not initialized")

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
            self.logger.error(f"[GeminiProvider] invoke failed: {e}")
            return None

    async def stream(self, messages: list) -> AsyncIterator[str]:
        if not self.client:
            raise Exception("Gemini client is not initialized")
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
            self.logger.error(f"[GeminiProvider] stream failed: {e}")
            raise e

    def embed_text(self, document_type: str, document_content: str = None):
        if not self.embedding_model_id:
            raise RuntimeError("Call set_embedding_model() before embed_text().")

        try:
            vector = self.embedding_model.embed_query(document_content)
            return vector
        except Exception as e:
            self.logger.error(f"[GeminiProvider] embed_text failed: {e}")
            return None

    def construct_prompt(self, prompt: str, role: str):
        return {"role": role, "content": prompt}

    def bind_tools(self, tools: List[BaseTool]):
        if not self.client:
            self.logger.error("Gemini Client is not initialized")
            raise Exception("Gemini client is not initialized")

        self.client = self.client.bind_tools(tools)
        self.logger.info(f"[GeminiProvider] Bound {len(tools)} tools to model.")

    def with_structured_output(self, schema: Any, **kwargs):
        """
        Bind a Pydantic output schema to the LLM.

        This is required for LangGraph Studio to automatically generate
        structured output nodes.
        """
        if not self.client:
            raise RuntimeError("Gemini client is not initialized. Call set_generation_model() first.")

        self.client = self.client.with_structured_output(schema, **kwargs)
        self.logger.info(f"[GeminiProvider] Bound structured output schema.")
        return self