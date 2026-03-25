from typing import List, Annotated, Dict
from pydantic import BaseModel, Field

class ResourceCuratorOutput(BaseModel):
    topic: Annotated[str, Field(description="Topic of the resource")]
    resources: Annotated[Dict[str, str], Field(description="Dictionary of resources for the topic {resource_name: resource_link}")]
    