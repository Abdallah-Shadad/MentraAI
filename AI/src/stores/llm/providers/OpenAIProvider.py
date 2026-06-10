import logging
from typing import List, Optional, Any, AsyncIterator

from langchain_openai import ChatOpenAI, OpenAIEmbeddings
from langchain_core.messages import HumanMessage, AIMessage, SystemMessage
from langchain_core.tools import BaseTool

from ..LLMInterface import LLMInterface
from ..LLMEnums import DocumentType,OpenAIEnums


class OpenAIProvider(LLMInterface):

    def __init__(self, api_key:str, base_url:str,
                max_output_tokens:int=100000,
                temperature:float=0.1,
                ):
        # Base Configuration
        self.api_key = api_key
        self.base_url = base_url if base_url else ""
        self.max_output_tokens = max_output_tokens
        self.temperature = temperature

        # Name of model 
        self.generation_model_id = None
        
        # embedding model
        self.embedding_model_id = None
        self.embedding_size=None

        self.enums = OpenAIEnums

        self.client = None
        self.logger = logging.getLogger("uvicorn.error")
    
    def set_generation_model(self,model_id:str):
        """Create Client with Specfic model_id"""
        self.generation_model_id=model_id
        self.client = ChatOpenAI(
            model=self.generation_model_id,
            openai_api_key=self.api_key,
            max_tokens=self.max_output_tokens,
            temperature=self.temperature,
            base_url=self.base_url,
        )
    
    def set_embedding_model(self, model_id: str, embedding_size: int):
        self.embedding_model_id=model_id
        self.embedding_size=embedding_size

    def invoke(self,prompt:str,chat_history:list=[],max_output_tokens:int=None,temperature:float=None):
        self.logger.info(f"Generating text using #OpenAIProvider: {self.generation_model_id}")

        if not self.client:
            self.logger.error("OpenAI Client is not intialized")
            raise Exception("OpenAI client is not Intialized")
        
        if not self.generation_model_id:
            self.logger.error("Generation model is not set")
            raise Exception("Generation model is not set")

        chat_history.append(
            self.construct_prompt(prompt=prompt,role=self.enums.USER.value)
        )

        try:
            response = self.client.invoke(chat_history)
            return response.content
        except Exception as e:
            self.logger.error(f"[OpenAIProvider] generate_text failed: {e}")
            return None

    async def stream(self, messages: list) -> AsyncIterator[str]:
        if not self.client:
            raise Exception("OpenAI client is not initialized")
        if not self.generation_model_id:
            raise Exception("Generation model is not set")
            
        try:
            async for chunk in self.client.astream(messages):
                if chunk.content:
                    yield chunk.content
        except Exception as e:
            self.logger.error(f"[OpenAIProvider] stream failed: {e}")
            raise e


    def embed_text(self,document_type:str,document_content:str=None):
        if not self.embedding_model_id:
            raise RuntimeError("Call set_embedding_model() before embed_text().")

        try:
            vector = self.embedding_model.embed_query(document_content)
            return vector
        except Exception as e:
            self.logger.error(f"[OpenAIProvider] embed_text failed: {e}")
            return None

    def construct_prompt(self,prompt:str,role:str):
        return {"role":role,"content":prompt}


    def bind_tools(self,tools: List[BaseTool]):
        if not self.client:
            self.logger.error("OpenAI Client is not intialized")
            raise Exception("OpenAI client is not Intialized")
        
        self.client = self.client.bind_tools(tools)
        self.logger.info(f"[OpenAIProvider] Bound {len(tools)} tools to model.")

    def with_structured_output(self, schema: Any, **kwargs):
        """
        Bind a Pydantic output schema to the LLM.

        This is required for LangGraph Studio to automatically generate
        structured output nodes.
        """
        if not self.client:
            raise RuntimeError("OpenAI client is not initialized. Call set_generation_model() first.")

        self.client = self.client.with_structured_output(schema, **kwargs)
        self.logger.info(f"[OpenAIProvider] Bound structured output schema.")
        return self
