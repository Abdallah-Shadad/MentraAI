from typing import List, Annotated
from pydantic import BaseModel, Field

class AdaptationEngineOutput(BaseModel):
    topic: Annotated[str, Field(description="Topic of the resource")]
    resources: Annotated[List[str], Field(description="List of resources for the topic")]
    Where_in_stage_will_add_resource: Annotated[str, Field(description="Where in the stage will add the resource")] 
    
