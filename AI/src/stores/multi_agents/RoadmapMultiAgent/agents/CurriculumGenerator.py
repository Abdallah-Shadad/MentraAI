"""
RoadmapMultiAgent/agents/CurriculumGenerator.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas import CurriculumOutput
from .prompts.CurriculumAgentPrompt import SYSTEM_PROMPT


class CurriculumGenerator(BaseWorkerAgent):
    """
    Roadmap Agent — Curriculum Generator
    =====================================
    Receives the profile analysis output and generates a personalised,
    stage-by-stage learning curriculum for the requested career track.

    State keys consumed  : ``career_track``, ``difficulty_level``,
                           ``skill_gaps``, ``prerequisite_analysis``,
                           ``estimated_duration_weeks``, ``weekly_hours``

    State keys produced  : ``curriculum``, ``curriculum_agent_done``
    """

    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

    # ── AgentInterface: identity ────────────────────────────────────────

    def get_agent_type(self) -> AgentType:
        return AgentType.CURRICULUM_GENERATOR.value

    # ── BaseWorkerAgent: hooks ──────────────────────────────────────────

    def _output_key(self) -> str:     return "curriculum"
    def _done_key(self) -> str:       return "curriculum_agent_done"
    def _output_schema(self) -> type: return CurriculumOutput

    # ── AgentInterface: message builder ────────────────────────────────

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        skill_gaps = state.get("skill_gaps", [])
        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"Career track     : {state.get('career_track', 'Not specified')}\n"
                    f"Difficulty level : {state.get('difficulty_level', 'beginner')}\n"
                    f"Skill gaps       : {', '.join(skill_gaps) if skill_gaps else 'None'}\n"
                    f"Weekly hours     : {state.get('weekly_hours', 10)}\n"
                    f"Prerequisite info: {state.get('prerequisite_analysis', {})}\n\n"
                    "Please generate the personalised curriculum JSON now."
                )
            ),
        ]
