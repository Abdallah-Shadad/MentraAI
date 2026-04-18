from typing import List, Annotated, Dict, Any
from pydantic import BaseModel, Field

class ProfileAnalyzerOutput(BaseModel):
    user_id: Annotated[str, Field(description="ID of the user")]
    career_track: Annotated[str, Field(description="Career track of the user")]
    skills: Annotated[List[str], Field(description="List of skills of the user")]
    difficulty_level: str        # "beginner" | "intermediate" | "advanced"
    skill_gaps: List[str]        # e.g. ["python", "machine learning"]
    prerequisite_analysis: Dict[str, Any]  # {"has_python": true, ...}
    estimated_duration_weeks: int
    ProfileAnalyzer_Summary: Annotated[str, Field(description="Summary of the user")]

# experience need to remove if use in RoadmapState