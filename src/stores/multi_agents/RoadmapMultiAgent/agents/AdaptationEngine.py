

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

    def __init__(self, config=None, llm=None, tools=None):
        # Default to ADAPTION_TOOLS; caller may inject different/mock tools.
        super().__init__(config=config, llm=llm, tools=tools if tools is not None else ADAPTION_TOOLS)
        # Override the base _system_prompt with the file-level constant so
        # set_system_prompt() callers also start from the correct default.
        self._system_prompt = SYSTEM_PROMPT


    def get_agent_type(self) -> AgentType:
        return AgentType.ADAPTATION_ENGINE.value

    def _output_key(self) -> str:     return "adapted_curriculum"
    def _done_key(self) -> str:       return "adaptation_agent_done"
    def _output_schema(self) -> type: return AdaptationEngineOutput

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        stage_id         = state.get("stage_id", "unknown")
        topic            = state.get("topic", "unknown")
        difficulty_level = state.get("difficulty_level", "beginner")
        learner_progress = state.get("learner_progress", {})
        curriculum       = state.get("curriculum", {})

        # Pull quiz details from learner_progress (set by adaptation endpoint)
        score            = learner_progress.get("score", "N/A")
        quiz_answers     = learner_progress.get("quiz_user_answers", {})
        quiz_result      = learner_progress.get("quiz_user_result", {})

        human_content = (
            f"Stage ID         : {stage_id}\n"
            f"Topic            : {topic}\n"
            f"Difficulty level : {difficulty_level}\n"
            f"Quiz score       : {score}% (FAILED — below 50%)\n\n"
            f"Quiz answers     :\n{quiz_answers}\n\n"
            f"Quiz result      :\n{quiz_result}\n\n"
            f"Current curriculum (for context):\n{curriculum}\n\n"
            f"Learner progress :\n{learner_progress}\n\n"
            "Please analyse the failure, identify struggling topics, search for "
            "remedial resources using the search_remedial_resources tool, and "
            "return the adaptation recommendations"
            "you must use the tool just once and search for video or documentaion that the user read it once and solve all his gaps in quiz. JUST ONCE!!"
        )

        return [
            SystemMessage(content=self._system_prompt),
            HumanMessage(content=human_content),
        ]
