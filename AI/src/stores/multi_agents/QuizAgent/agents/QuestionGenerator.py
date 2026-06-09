"""
QuizAgent/agents/QuestionGenerator.py
======================================
Quiz Agent — Question Generator

Receives a curriculum stage (topics + learning objectives) and generates
a comprehensive quiz with multiple-choice questions, correct answers,
explanations, and progressive hints.

State keys consumed  : ``stage_id``, ``stage_name``, ``topics``,
                       ``learning_objectives``, ``difficulty_level``,
                       ``career_track``
State keys produced  : ``quiz_questions``, ``question_generator_done``
"""

from typing import Any, Dict, List

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage

from ...AgentEnums import AgentType
from ...BaseWorkerAgent import BaseWorkerAgent
from .schemas.QuestionGenerator import QuestionGeneratorOutput
from .prompts.QuestionGeneratorPrompt import SYSTEM_PROMPT


class QuestionGenerator(BaseWorkerAgent):
    """
    Quiz Agent — Question Generator
    =================================
    Generates quiz questions with correct answers, hints, and explanations
    for a given curriculum stage.
    """

    _DEFAULT_SYSTEM_PROMPT = SYSTEM_PROMPT

    # ── AgentInterface: identity ────────────────────────────────────────

    def get_agent_type(self) -> AgentType:
        return AgentType.QUESTION_GENERATOR.value

    # ── BaseWorkerAgent: hooks ──────────────────────────────────────────

    def _output_key(self) -> str:     return "quiz_questions"
    def _done_key(self) -> str:       return "question_generator_done"
    def _output_schema(self) -> type: return QuestionGeneratorOutput

    # ── AgentInterface: message builder ────────────────────────────────

    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        topics = state.get("topics", [])
        learning_objectives = state.get("learning_objectives", {})

        # Format learning objectives for display
        if isinstance(learning_objectives, dict):
            objectives_str = "\n".join(
                f"  - {obj}: {desc}" for obj, desc in learning_objectives.items()
            )
        elif isinstance(learning_objectives, list):
            objectives_str = "\n".join(f"  - {obj}" for obj in learning_objectives)
        else:
            objectives_str = str(learning_objectives)

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
                    "Please generate the quiz questions JSON now."
                )
            ),
        ]
