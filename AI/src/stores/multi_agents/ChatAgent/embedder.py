from helpers.config import get_settings
from stores.llm.providers.CoHereProvider import CoHereProvider
from stores.llm.LLMEnums import DocumentTypeEnum
import logging


settings = get_settings()
class Embedder:
    def __init__(self):
        self.client = CoHereProvider(api_key=settings.COHERE_API_KEY)
        self.embedding_model_id = settings.EMBEDDING_MODEL_NAME
        self.client.set_embedding_model(self.embedding_model_id,settings.EMBEDDING_SIZE)

    async def get_embedding(self,text: str) -> list[float]:
        """
        Convert a text string into a vector embedding.
        """
        text = text.strip().replace("\n", " ")
        response = self.client.embed_text(
            document_type=DocumentTypeEnum.DOCUMENT.value,
            document_content=text
        )
        return response[0]

    
if __name__ == "__main__":
    embedder = Embedder()
    embedding = embedder.get_embedding("Hello, how are you?")
    print(embedding)