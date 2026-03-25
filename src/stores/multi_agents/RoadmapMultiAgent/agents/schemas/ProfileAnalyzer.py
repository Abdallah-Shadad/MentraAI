from typing import List, Annotated
from pydantic import BaseModel, Field

class ProfileAnalyzerOutput(BaseModel):
    name: Annotated[str, Field(description="Name of the user")]
    skills: Annotated[List[str], Field(description="List of skills of the user")]
    experience: Annotated[str, Field(description="Experience of the user")]
    summary: Annotated[str, Field(description="Summary of the user")]