from typing import List, Annotated
from pydantic import BaseModel, Field

class Resource(BaseModel):
    name: Annotated[str, Field(description="Name of the resource")]
    url: Annotated[str, Field(description="URL of the resource")]
    type: Annotated[str, Field(description="Type of the resource")]

class AdaptationEngineOutput(BaseModel):
    stage_id: Annotated[str, Field(description="ID of the stage where will add to Roadmap")]
    struggling_topics: Annotated[str, Field(description="Struggling topic what user get lower Accuarcy when solve Quiz")]
    recommended_resources: Annotated[Resource, Field(description="Recommended resources Based on User point Failure")]
    summary: Annotated[str, Field(description="Summary of the stage that will Encapsulate the Struggling topics and Recommended resources For user and Encourage him to work more")]
