"""
RoadmapMultiAgent/agents/AdaptationEngine.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.AdaptationEngine import AdaptationEngineOutput
from .prompts.AdaptationEnginePrompt import SYSTEM_PROMPT

class AdaptationEngine(BaseWorkerAgent):
    """
    Roadmap Agent — Adaptation Engine
    ====================================
    Dynamically adjusts the learning roadmap based on quiz performance.

    State keys consumed  : ``learner_progress``, ``curriculum``
    State keys produced  : ``adapted_curriculum``, ``adaptation_agent_done``
    """

    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

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
                    f"User Background : {state.get('user_background', 'Not specified')}"
                    f"Career track      : {state.get('career_track', 'Not specified')}\n"
                    f"Current stage     : {state.get('current_stage', 'Not specified')}\n"
                    f"Difficulty level  : {state.get('difficulty_level', 'Not specified')}\n"
                    f"Full curriculum   : {state.get('curriculum', 'Not specified')}\n"
                    "Please analyse and return roadmap adaptation recommendations."
                )
            ),
        ]
