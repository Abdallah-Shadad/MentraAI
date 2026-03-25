from pydantic_settings import BaseSettings
from typing import List
class Settings(BaseSettings):
    APP_NAME: str
    APP_VERSION: str

    # secret Key
    GEMINI_API_KEY: str
    
    # Langsmith
    LANGSMITH_TRACING: bool
    LANGSMITH_ENDPOINT: str
    LANGSMITH_API_KEY: str
    LANGSMITH_PROJECT: str

    # tavily
    TAVILY_API_KEY: str
        

    class Config:
        env_file = ".env"
        env_file_encoding = "utf-8"
        extra = "ignore"

def get_settings() -> Settings:
    return Settings()
    