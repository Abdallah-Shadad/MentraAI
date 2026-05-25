from typing import List, Annotated
from pydantic import BaseModel, Field

class LearningResource(BaseModel):
    title: Annotated[str, Field(description="Title of the learning resource")]
    url: Annotated[str, Field(description="Direct URL to the resource")]
    type: Annotated[str, Field(description="Type of resource (e.g., 'video', 'article', 'course', 'documentation')")]
    quality_score: Annotated[float, Field(description="Estimated quality score from 0.0 to 10.0")]

class TopicResources(BaseModel):
    topic_name: Annotated[str, Field(description="The exact name of the topic from the stage")]
    videos: Annotated[List[LearningResource], Field(default_factory=list, description="Video tutorials and courses covering this topic")]
    articles: Annotated[List[LearningResource], Field(default_factory=list, description="Articles, blog posts, and text tutorials")]
    documentation: Annotated[List[LearningResource], Field(default_factory=list, description="Official documentation and references")]

class ResourceCuratorOutput(BaseModel):
    stage_id: Annotated[str, Field(description="ID of the stage being curated")]
    topics_resources: Annotated[List[TopicResources], Field(description="A list mapping every topic in the stage to its specific learning resources")]