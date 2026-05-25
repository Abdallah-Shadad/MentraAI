"""
ProjectAgent/agents/ProjectRecommender.py
==========================================
Project Agent — Project Recommender

Receives a curriculum stage context (what the learner just completed,
what's coming next) and recommends 1-3 practical, job-market-relevant
projects — either scoped to a single stage or split across multiple stages.

State keys consumed  : ``career_track``, ``stage_id``, ``stage_name``,
                       ``topics``, ``learning_objectives``,
                       ``difficulty_level``, ``completed_stages``,
                       ``upcoming_stages``
State keys produced  : ``project_recommendations``, ``project_recommender_done``
"""

from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.ProjectRecommender import ProjectRecommenderOutput
from .prompts.ProjectRecommenderPrompt import SYSTEM_PROMPT


class ProjectRecommender(BaseWorkerAgent):
    """
    Project Agent — Project Recommender
    =====================================
    Generates job-market-aligned project recommendations with milestones
    mapped to curriculum stages.
    """

    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

    # ── AgentInterface: identity ────────────────────────────────────────

    def get_agent_type(self) -> AgentType:
        return AgentType.PROJECT_RECOMMENDER.value

    # ── BaseWorkerAgent: hooks ──────────────────────────────────────────

    def _output_key(self) -> str:     return "project_recommendations"
    def _done_key(self) -> str:       return "project_recommender_done"
    def _output_schema(self) -> type: return ProjectRecommenderOutput

    # ── AgentInterface: message builder ────────────────────────────────

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        topics = state.get("topics", [])
        learning_objectives = state.get("learning_objectives", {})
        completed_stages = state.get("completed_stages", [])
        upcoming_stages = state.get("upcoming_stages", [])

        # Format learning objectives for display
        if isinstance(learning_objectives, dict):
            objectives_str = "\n".join(
                f"  - {obj}: {desc}" for obj, desc in learning_objectives.items()
            )
        elif isinstance(learning_objectives, list):
            objectives_str = "\n".join(f"  - {obj}" for obj in learning_objectives)
        else:
            objectives_str = str(learning_objectives)

        # Format completed stages
        if completed_stages:
            completed_str = "\n".join(
                f"  - {s.get('id', s) if isinstance(s, dict) else s}: "
                f"{s.get('name', '') if isinstance(s, dict) else ''} "
                f"(topics: {', '.join(s.get('topics', [])) if isinstance(s, dict) else 'N/A'})"
                for s in completed_stages
            )
        else:
            completed_str = "  None (this is the first stage)"

        # Format upcoming stages
        if upcoming_stages:
            upcoming_str = "\n".join(
                f"  - {s.get('id', s) if isinstance(s, dict) else s}: "
                f"{s.get('name', '') if isinstance(s, dict) else ''} "
                f"(topics: {', '.join(s.get('topics', [])) if isinstance(s, dict) else 'N/A'})"
                for s in upcoming_stages
            )
        else:
            upcoming_str = "  None (this is the final stage)"

        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"Career Track         : {state.get('career_track', 'Not specified')}\n"
                    f"Stage ID             : {state.get('stage_id', 'unknown')}\n"
                    f"Stage Name           : {state.get('stage_name', 'unknown')}\n"
                    f"Difficulty Level     : {state.get('difficulty_level', 'beginner')}\n"
                    f"Topics               : {', '.join(topics) if topics else 'Not specified'}\n"
                    f"Learning Objectives  :\n{objectives_str}\n\n"
                    f"Completed Stages     :\n{completed_str}\n\n"
                    f"Upcoming Stages      :\n{upcoming_str}\n\n"
                    "Based on this context, recommend practical projects that are "
                    "directly aligned with current job market demands. "
                    "Please generate the project recommendations JSON now."
                )
            ),
        ]
