import json
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.TrackRecommender import TrackRecommenderOutput
from .prompts.TrackRecommenderPrompt import SYSTEM_PROMPT


class TrackRecommender(BaseWorkerAgent):
    """
    Light Agent — Track Recommender
    ================================
    Analyses a user's profile (which may be partially complete) and
    recommends the 3-5 most suitable tech career tracks.

    State keys consumed  : ``profile`` (dict with optional user fields)
    State keys produced  : ``track_recommendations``, ``track_recommender_done``
    """

    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

    # ── AgentInterface: identity ────────────────────────────────────────

    def get_agent_type(self) -> AgentType:
        return AgentType.TRACK_RECOMMENDER.value

    # ── BaseWorkerAgent: hooks ──────────────────────────────────────────

    def _output_key(self) -> str:    return "track_recommendations"
    def _done_key(self) -> str:      return "track_recommender_done"
    def _output_schema(self) -> type: return TrackRecommenderOutput

    # ── BaseWorkerAgent: message builder ────────────────────────────────

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        """
        Build a [System, Human] message pair from the user profile in state.

        The profile dict may contain any subset of the following keys:
        Age, EdLevel, YearsCode, WorkExp, Employment, RemoteWork,
        Industry, OrgSize, AISelect, current_skills, future_skills.

        Missing keys are labelled "Not provided" so the LLM can still
        reason about partial data.
        """
        profile = state.get("profile", {})

        # Build a clean text representation of whatever profile data exists
        profile_lines = []

        field_map = {
            "Age":            "Age",
            "EdLevel":        "Education Level",
            "YearsCode":      "Years of Coding Experience",
            "WorkExp":        "Years of Work Experience",
            "Employment":     "Employment Status",
            "RemoteWork":     "Remote Work Preference",
            "Industry":       "Industry",
            "OrgSize":        "Organisation Size",
            "AISelect":       "AI Tool Usage",
        }

        for key, label in field_map.items():
            value = profile.get(key)
            if value is not None and value != "":
                profile_lines.append(f"  {label}: {value}")
            else:
                profile_lines.append(f"  {label}: Not provided")

        # Skills (lists)
        current_skills = profile.get("current_skills", [])
        future_skills = profile.get("future_skills", [])

        if current_skills:
            profile_lines.append(f"  Current Skills: {', '.join(current_skills)}")
        else:
            profile_lines.append("  Current Skills: Not provided")

        if future_skills:
            profile_lines.append(f"  Future Skills (wants to learn): {', '.join(future_skills)}")
        else:
            profile_lines.append("  Future Skills: Not provided")

        profile_text = "\n".join(profile_lines)

        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(
                content=(
                    f"User ID: {state.get('user_id', 'unknown')}\n\n"
                    f"USER PROFILE:\n{profile_text}\n\n"
                    f"Please analyse this profile and recommend the best tech career tracks."
                )
            ),
        ]
