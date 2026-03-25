"""
RoadmapMultiAgent/agents/ResourceCurator.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.ResourceCurator import ResourceCuratorOutput

class ResourceCurator(BaseWorkerAgent):
    """
    Roadmap Agent — Resource Curator
    ==================================
    Finds and curates high-quality learning materials for each curriculum stage.

    State keys consumed  : ``current_stage``, ``difficulty_level``
    State keys produced  : ``resources``, ``resource_agent_done``
    """

    _DEFAULT_SYSTEM_PROMPT = """You are an expert learning-resource curator.
For each stage in the provided curriculum, find high-quality materials.
Return a JSON:
{
  "<stage_id>": [
    {"title": "...", "url": "...", "type": "video|article|documentation", "quality_score": 0.0-1.0}
  ]
}
Prioritise official docs and trusted educational platforms.
"""

    def get_agent_type(self) -> AgentType:
        return AgentType.RESOURCE_CURATOR.value

    def _output_key(self) -> str:     return "resources"
    def _done_key(self) -> str:       return "resource_agent_done"
    def _output_schema(self) -> type: return ResourceCuratorOutput

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"Difficulty level : {state.get('difficulty_level', 'beginner')}\n"
                    f"Stage to curate  : {state.get('current_stage', {})}\n\n"
                    "Please curate 3-5 high-quality resources for this single stage only."
                )
            ),
        ]


# =============================================================================
