

"""
RoadmapMultiAgent/agents/AdaptationEngine.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.AdaptationEngine import AdaptationEngineOutput
from .prompts.AdaptationEnginePrompt import SYSTEM_PROMPT
from .tools.AdaptionEngineTools import ADAPTION_TOOLS

class AdaptationEngine(BaseWorkerAgent):
    """
    Roadmap Agent — Adaptation Engine
    ====================================
    Dynamically adjusts the learning roadmap based on quiz performance.

    State keys consumed  : ``learner_progress``, ``curriculum``
    State keys produced  : ``adapted_curriculum``, ``adaptation_agent_done``
    """

    _DEFAULT_SYSTEM_PROMPT = """You are an adaptive learning specialist.
Analyse the learner's quiz performance and suggest roadmap adjustments.
Return a JSON:
{
  "struggling_topics": ["topic1", "..."],
  "recommended_resources": [{"title": "...", "url": "..."}],
  "stage_adjustments": [{"action": "reorder|insert_remedial", "stage_id": "...", "reason": "..."}],
  "summary": "brief explanation of changes"
}
"""

    def __init__(self, config=None, llm=None, tools=None):
        # Default to ADAPTION_TOOLS; caller may inject different/mock tools.
        super().__init__(config=config, llm=llm, tools=tools if tools is not None else ADAPTION_TOOLS)

    def get_agent_type(self) -> AgentType:
        return AgentType.ADAPTATION_ENGINE.value

    def _output_key(self) -> str:     return "adapted_curriculum"
    def _done_key(self) -> str:       return "adaptation_agent_done"
    def _output_schema(self) -> type: return AdaptationEngineOutput

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"Current curriculum  : {state.get('curriculum', {})}\n"
                    f"Learner progress    : {state.get('learner_progress', {})}\n\n"
                    "Please analyse and return roadmap adaptation recommendations."
                )
            ),
        ]
