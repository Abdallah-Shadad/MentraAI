from pydantic import BaseModel
from typing import List, Optional, Dict, Any, Annotated
from pydantic import Field

class CurriculumStage(BaseModel):
    id: Annotated[str, Field(description="Unique identifier for the stage")]
    name: Annotated[str, Field(description="Name of the stage")]
    topics: Annotated[List[str], Field(description="List of topics in the stage")]
    learning_objectives: Annotated[Dict[str, str], Field(description="Dictionary of learning objectives for the stage have key as objective and value as description")]
    estimated_weeks: Annotated[int, Field(description="Estimated number of weeks to complete the stage")]

class CurriculumOutput(BaseModel):
    stages: Annotated[List[CurriculumStage], Field(description="List of stages in the curriculum")]
    dependencies: Annotated[Dict[str, List[str]], Field(description="Dictionary of stage dependencies")]
    total_weeks: Annotated[int, Field(description="Total number of weeks to complete the curriculum")]
    