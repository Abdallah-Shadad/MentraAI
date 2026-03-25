from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas import ProfileAnalyzerOutput


class ProfileAnalyzer(BaseWorkerAgent):
    """
    Roadmap Agent — Profile Analyzer
    ==================================
    Analyses the user's background and identifies skill gaps
    relative to the requested career track.

    State keys consumed  : ``user_id``, ``career_track``
    State keys produced  : ``profile_analysis``, ``profile_agent_done``
    """

    _DEFAULT_SYSTEM_PROMPT = """You are an expert career and skills assessment specialist.
Analyse the user's background and career goal, then return a JSON:
{
  "difficulty_level": "beginner|intermediate|advanced",
  "skill_gaps": ["skill1", "skill2"],
  "prerequisite_analysis": {"has_python": true, ...},
  "estimated_duration_weeks": <int>
}
"""

    # ── AgentInterface: identity ────────────────────────────────────────

    def get_agent_type(self) -> AgentType:
        return AgentType.PROFILE_ANALYZER.value

    # ── BaseWorkerAgent: hooks ──────────────────────────────────────────

    def _output_key(self) -> str:   return "profile_analysis"
    def _done_key(self) -> str:     return "profile_agent_done"
    def _output_schema(self) -> type: return ProfileAnalyzerOutput

    # ── BaseWorkerAgent: Use it in invoke ────────────────────────────────

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"User ID      : {state.get('user_id', 'unknown')}\n"
                    f"Career Track : {state.get('career_track', 'Not specified')}"
                )
            ),
        ]