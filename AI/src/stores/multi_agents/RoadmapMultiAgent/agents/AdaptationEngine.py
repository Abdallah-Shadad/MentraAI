"""
RoadmapMultiAgent/agents/AdaptationEngine.py
"""
from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.AdaptationEngine import AdaptationEngineOutput
from .prompts.AdaptionEnginePrompt import SYSTEM_PROMPT
from .tools.AdaptionEngineTools import ADAPTION_TOOLS

class AdaptationEngine(BaseWorkerAgent):
    """
    Roadmap Agent — Adaptation Engine
    ====================================
    Activated exclusively when ``is_adaptation_mode = True`` (learner scored < 50 %).
    The Supervisor routes directly here, bypassing all other agents.

    State keys consumed
    -------------------
    ``stage_id``, ``topic``, ``difficulty_level``, ``learner_progress``,
    ``curriculum``

    State keys produced
    -------------------
    ``adapted_curriculum``  (AdaptationEngineOutput Pydantic object)
    ``adaptation_agent_done``  (bool → True on success)

    Tools
    -----
    ``search_remedial_resources`` — searches Tavily + YouTube for remedial content.
    Called once per struggling topic identified from the failed quiz questions.
    """

    # Kept for AgentProviderFactory introspection; actual prompt comes from the file.
    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

    def __init__(self, config=None, llm=None, tools=None, llm_manager=None):
        # Default to ADAPTION_TOOLS; caller may inject different/mock tools.
        super().__init__(config=config, llm=llm, tools=tools if tools is not None else ADAPTION_TOOLS, llm_manager=llm_manager)
        # Override the base _system_prompt with the file-level constant so
        # set_system_prompt() callers also start from the correct default.
        self._system_prompt = SYSTEM_PROMPT


    def get_agent_type(self) -> AgentType:
        return AgentType.ADAPTATION_ENGINE.value

    def _output_key(self) -> str:     return "adapted_curriculum"
    def _done_key(self) -> str:       return "adaptation_agent_done"
    def _output_schema(self) -> type: return AdaptationEngineOutput

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        learner_progress = state.get("learner_progress", {})
        curriculum       = state.get("curriculum", {})
        
        # Pull stage_id and stage_name from learner_progress (user's preferred schema) or fallback to state
        stage_id         = learner_progress.get("stage_id", state.get("stage_id", "unknown"))
        stage_name       = learner_progress.get("stage_name", state.get("stage_name", "unknown"))
        difficulty_level = state.get("difficulty_level", "beginner")

        # Pull quiz details from learner_progress
        score            = learner_progress.get("score", "N/A")
        failed_questions = learner_progress.get("failed_questions", [])

        # Format failed questions beautifully as clean text
        formatted_failed = ""
        for i, q in enumerate(failed_questions, 1):
            if isinstance(q, dict):
                q_text = q.get("question", "")
                u_ans = q.get("user_answer", "")
                c_ans = q.get("correct_answer", "")
            else:
                q_text = getattr(q, "question", "")
                u_ans = getattr(q, "user_answer", "")
                c_ans = getattr(q, "correct_answer", "")
            formatted_failed += f"Question {i}: {q_text}\n   - User's Answer: {u_ans}\n   - Correct Answer: {c_ans}\n\n"

        human_content = (
            f"Stage ID         : {stage_id}\n"
            f"Stage Name       : {stage_name}\n"
            f"Difficulty level : {difficulty_level}\n"
            f"Quiz score       : {score}% (FAILED — below 50%)\n\n"
            f"Failed Questions :\n{formatted_failed}\n"
            f"Current curriculum (for context):\n{curriculum}\n\n"
            "Please analyse the failure, identify struggling topics, search for "
            "remedial resources using the search_remedial_resources tool, and "
            "return the adaptation recommendations. "
            "You MUST call the search tool EXACTLY ONCE. Craft a highly targeted, unified topic search query that directly combines the specific concepts the user failed (e.g., 'Git branching and fetch pull' or 'CSS Grid layout and Flexbox alignment' rather than generic technology names) to fetch extremely relevant remedial resources in a single call."
        )

        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(content=human_content),
        ]
