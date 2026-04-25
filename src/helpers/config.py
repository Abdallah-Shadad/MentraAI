from pydantic_settings import BaseSettings
from typing import List
from pathlib import Path

# Resolve .env from project root regardless of working directory
# src/helpers/config.py -> src/helpers -> src -> project_root
ENV_FILE = Path(__file__).resolve().parent.parent.parent / ".env"

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

    # youtube
    YOUTUBE_API_KEY: str
        

    class Config:
        env_file = str(ENV_FILE)
        env_file_encoding = "utf-8"
        extra = "ignore"

def get_settings() -> Settings:
    return Settings()
    