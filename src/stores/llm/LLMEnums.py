from enum import Enum

class LLMEnums(Enum):
    OPENAI = "OPENAI"
    GEMINI = "GEMINI"
    COHERE = "COHERE"
    
class OpenAIEnums(Enum):
    SYSTEM = "system"
    USER = "user"
    ASSISTANT = "assistant"

class DocumentType(Enum):
    DOCUMENT = "document"
    QUERY = "query"

class CohereEnums(Enum):
    SYSTEM = "SYSTEM"
    USER = "USER"
    ASSISTANT = "CHATBOT"

    DOCUMENT= "search_document"
    QUERY= "search_query"

class OllamaEnums(Enum):
    SYSTEM = "system"
    USER = "user"
    ASSISTANT = "assistant"

    DOCUMENT = "document"
    QUERY = "query"

class GeminiEnums(Enum):
    SYSTEM = "system"
    USER = "user"
    ASSISTANT = "model"   # Gemini uses "model" for assistant

    DOCUMENT = "retrieval_document"
    QUERY = "retrieval_query"


class DocumentTypeEnum(Enum):
    DOCUMENT = "document"
    QUERY = "query"