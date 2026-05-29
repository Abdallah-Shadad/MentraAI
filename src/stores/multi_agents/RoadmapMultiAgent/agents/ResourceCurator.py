"""
RoadmapMultiAgent/agents/ResourceCurator.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.ResourceCurator import ResourceCuratorOutput
from .tools.ResourceCuratorTools import RESOURCE_TOOLS

class ResourceCurator(BaseWorkerAgent):
    """
    Roadmap Agent — Resource Curator
    ==================================
    Finds and curates high-quality learning materials for EACH TOPIC inside a stage.
    Every topic gets its own dedicated video AND article resource.

    State keys consumed  : ``current_stage``, ``difficulty_level``
    State keys produced  : ``stage_resources``, ``resource_agent_done``
    """

    def __init__(self, config=None, llm=None, tools=None, llm_manager=None):
        super().__init__(config=config, llm=llm, tools=tools if tools is not None else RESOURCE_TOOLS, llm_manager=llm_manager)

    _DEFAULT_SYSTEM_PROMPT = """You are an elite learning-resource curator. Your job is to find the absolute BEST learning materials for each individual topic.

CRITICAL RULES:
1. You MUST call the `search_learning_resources` tool ONCE PER TOPIC in the stage.
2. Every single topic must have its OWN dedicated resources — do NOT share resources between topics.
3. Categorize the found resources strictly into three lists per topic: `videos`, `articles`, and `documentation`.
4. STRICT URL VALIDATION: Any URL containing 'youtube.com' or 'youtu.be' MUST be placed in the `videos` list. NEVER put a video link in `articles` or `documentation`.
5. If you cannot find a valid text-based article for a topic, leave the `articles` list empty rather than inserting a video.
6. LIMIT the output: Return EXACTLY 1 video and EXACTLY 1 article per topic. Never return more than one. Choose the single highest-rated resource of each type.
7. NEVER hallucinate or invent URLs. Only use real resources returned by the tool.
8. After calling the tool for ALL topics, compile the final structured output using the ResourceCuratorOutput schema.

Your output must map EVERY topic in the stage to its specific curated resources, separated by type.
"""

    def get_agent_type(self) -> AgentType:
        return AgentType.RESOURCE_CURATOR.value

    def _output_key(self) -> str:     return "stage_resources"
    def _done_key(self) -> str:       return "resource_agent_done"
    def _output_schema(self) -> type: return ResourceCuratorOutput

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        current_stage = state.get("current_stage", {})
        topics = current_stage.get("topics", [])
        difficulty = state.get("difficulty_level", "beginner")

        topic_list = "\n".join(f"  - {t}" for t in topics)

        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"Stage ID         : {current_stage.get('id', 'unknown')}\n"
                    f"Stage Name       : {current_stage.get('name', 'unknown')}\n"
                    f"Difficulty Level : {difficulty}\n\n"
                    f"Topics that EACH need their own dedicated resources:\n{topic_list}\n\n"
                    f"IMPORTANT: Call `search_learning_resources` ONCE for EACH of the {len(topics)} topics above. "
                    f"Every topic must have EXACTLY 1 video and EXACTLY 1 article in its resources (no more, no less)."
                )
            ),
        ]

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """
        Executes the agent and programmatically enforces that every topic 
        has EXACTLY at most 1 video, 1 article, and 1 documentation resource
        by choosing the highest quality score from the model's choices.
        """
        res = super().invoke(state)
        stage_resources = res.get("stage_resources")
        if stage_resources and hasattr(stage_resources, "topics_resources"):
            for topic in stage_resources.topics_resources:
                if len(topic.videos) > 1:
                    topic.videos = sorted(topic.videos, key=lambda x: getattr(x, "quality_score", 0.0) or 0.0, reverse=True)[:1]
                if len(topic.articles) > 1:
                    topic.articles = sorted(topic.articles, key=lambda x: getattr(x, "quality_score", 0.0) or 0.0, reverse=True)[:1]
                if len(topic.documentation) > 1:
                    topic.documentation = sorted(topic.documentation, key=lambda x: getattr(x, "quality_score", 0.0) or 0.0, reverse=True)[:1]
            
            # Sync back modified resources to state dict keys
            output_dict = stage_resources.model_dump() if hasattr(stage_resources, "model_dump") else stage_resources.dict() if hasattr(stage_resources, "dict") else {}
            res.update(output_dict)
            res["stage_resources"] = stage_resources

        return res


# =============================================================================
