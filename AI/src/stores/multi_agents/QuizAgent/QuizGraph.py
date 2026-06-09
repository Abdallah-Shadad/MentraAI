"""
QuizGraph — Multi-Agent Supervisor Graph (Question Generation)
================================================================
Implements the Quiz Supervisor pattern using LangGraph.

This graph is responsible ONLY for generating quiz questions with
correct answers and progressive hints. The backend server handles
answer evaluation, hint serving, and all other quiz logic.

Architecture
------------
                  ┌──────────────────┐
        START ───►│   SUPERVISOR     │
                  │  (routes to      │
                  │   next agent)    │
                  └────────┬─────────┘
                           │
                ┌──────────▼──────────┐
                │ QuestionGenerator   │
                │ (generates quiz     │
                │  with answers +     │
                │  hints)             │
                └──────────┬──────────┘
                           │
                ┌──────────▼──────────┐
                │ ResponseFormatter   │
                │ (builds API JSON)   │
                └──────────┬──────────┘
                           │
                          END

Flow
----
  START → supervisor → question_generator
        → supervisor → response_formatter → END
"""

import logging
import json
from typing import Any, Dict, List, Optional, TypedDict, Annotated

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage, AIMessage
from langgraph.graph import StateGraph, END, START
from langgraph.graph.message import add_messages

from ..AgentEnums import AgentType
from ..AgentProviderFactory import AgentProviderFactory
from ...graph import GraphInterface
from ...graph.GraphEnums import GraphType, NodeName


# ══════════════════════════════════════════════════════════════════════════════
# 1. SHARED STATE
# ══════════════════════════════════════════════════════════════════════════════

class QuizState(TypedDict, total=False):
    """
    Shared state dictionary that flows through every node in the
    QuizGraph. All fields are optional (total=False) so individual
    agents only need to write the keys they produce.
    """

    # ── Inputs ─────────────────────────────────────────────────────────
    user_id: Annotated[str, "The unique identifier of the user"]
    career_track: Annotated[str, "The user's chosen career track"]
    stage_id: Annotated[str, "The curriculum stage to generate a quiz for"]
    stage_name: Annotated[str, "Human-readable name of the stage"]
    topics: Annotated[List[str], "Topics covered in this stage"]
    learning_objectives: Annotated[Any, "Learning objectives for this stage (dict or list)"]
    difficulty_level: Annotated[str, "Assessment difficulty: 'beginner', 'intermediate', 'advanced'"]
    num_questions: Annotated[int, "Number of questions to generate (default: 10)"]

    # ── QuestionGenerator outputs ──────────────────────────────────────
    quiz_questions: Annotated[Any, "The generated quiz questions with answers and hints. Produced by QuestionGenerator."]
    question_generator_done: Annotated[bool, "Signals completion of QuestionGenerator. Used by Supervisor for routing."]

    # ── Supervisor control ──────────────────────────────────────────────
    next_agent: Annotated[str, "Next agent to route to, or 'FINISH'. Set by Supervisor."]
    error: Annotated[Optional[str], "Error message if any node fails."]

    # ── Message history (LangGraph convention) ──────────────────────────
    messages: Annotated[List[BaseMessage], add_messages]

    # ── Final formatted response ────────────────────────────────────────
    api_response: Annotated[Optional[Dict[str, Any]], "Frontend-ready JSON built by ResponseFormatter node."]


# ══════════════════════════════════════════════════════════════════════════════
# 2. QUIZ GRAPH IMPLEMENTATION
# ══════════════════════════════════════════════════════════════════════════════

class QuizGraph(GraphInterface):
    """
    Quiz Multi-Agent Supervisor Graph — Question Generation Only.

    The AI server generates quiz questions with correct answers and hints.
    The backend server handles evaluation, hint delivery, and grading.

    Parameters
    ----------
    config        :  Project-wide config / settings object.
    agent_factory :  ``AgentProviderFactory`` that provides worker agents.
    llm           :  An LLMInterface-compatible provider for the supervisor node.
    """

    def __init__(
        self,
        config: Any,
        agent_factory: Optional[AgentProviderFactory] = None,
        llm: Any = None,
    ):
        self.config = config
        self.agent_factory = agent_factory
        self.llm = llm
        self.logger = logging.getLogger("uvicorn.error")

        # Internal graph + compiled app
        self._graph: Optional[StateGraph] = None
        self._app: Any = None

        # Worker agent — created from agent_factory in _setup_agents()
        self._question_generator = None

    # ── GraphInterface: identity ───────────────────────────────────────

    def get_graph_type(self) -> GraphType:
        return GraphType.QUIZ_GRAPH.value

    def get_state_schema(self) -> type:
        return QuizState

    # ── GraphInterface: build & compile ────────────────────────────────

    def build(self) -> "QuizGraph":
        """Wire all nodes, edges, and conditional routing."""
        self._setup_agents()

        workflow = StateGraph(QuizState, name="Quiz Multi-Agent System")

        # ── Register nodes ─────────────────────────────────────────────
        workflow.add_node(NodeName.SUPERVISOR.value,          self._supervisor_node)
        workflow.add_node(NodeName.QUESTION_GENERATOR.value,  self._question_generator_node)
        workflow.add_node(NodeName.RESPONSE_FORMATTER.value,  self._response_formatter_node)

        # ── Entry point ────────────────────────────────────────────────
        workflow.set_entry_point(NodeName.SUPERVISOR.value)

        # ── Conditional routing FROM supervisor ────────────────────────
        workflow.add_conditional_edges(
            NodeName.SUPERVISOR.value,
            self._router,
            {
                NodeName.QUESTION_GENERATOR.value:  NodeName.QUESTION_GENERATOR.value,
                NodeName.RESPONSE_FORMATTER.value:  NodeName.RESPONSE_FORMATTER.value,
                END:                                END,
            },
        )

        # ── QuestionGenerator loops back to supervisor ─────────────────
        workflow.add_edge(NodeName.QUESTION_GENERATOR.value, NodeName.SUPERVISOR.value)
        workflow.add_edge(NodeName.RESPONSE_FORMATTER.value, END)

        self._graph = workflow
        self.logger.info("[QuizGraph] Graph built successfully.")
        return self

    def compile(self) -> Any:
        """Compile the StateGraph into a runnable LangGraph application."""
        if self._graph is None:
            raise RuntimeError("Call build() before compile().")
        self._app = self._graph.compile()
        self.logger.info("[QuizGraph] Graph compiled successfully.")
        return self._app

    # ── GraphInterface: execution ──────────────────────────────────────

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """Run the full graph synchronously and return the final state."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before invoke().")
        self.logger.info(f"[QuizGraph] Invoking for user: {state.get('user_id')}")
        return self._app.invoke(state)

    def stream(self, state: Dict[str, Any]):
        """Run the graph and yield step-by-step state updates."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before stream().")
        self.logger.info(f"[QuizGraph] Streaming for user: {state.get('user_id')}")
        yield from self._app.stream(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Agent Setup
    # ══════════════════════════════════════════════════════════════════

    def _setup_agents(self):
        """Instantiate the QuestionGenerator via AgentProviderFactory."""
        if self.agent_factory is None:
            raise RuntimeError(
                "QuizGraph requires an AgentProviderFactory. "
                "Pass agent_factory= when constructing QuizGraph."
            )

        self._question_generator = self.agent_factory.create(
            AgentType.QUESTION_GENERATOR, llm=self.llm
        )

        self.logger.info("[QuizGraph] QuestionGenerator agent initialised.")

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Graph Nodes
    # ══════════════════════════════════════════════════════════════════

    def _supervisor_node(self, state: QuizState) -> Dict[str, Any]:
        """
        Deterministic supervisor — routes to QuestionGenerator first,
        then to ResponseFormatter once questions are generated.
        """
        self.logger.info("[Quiz Supervisor] Deciding next agent...")

        if not state.get("question_generator_done"):
            next_agent = NodeName.QUESTION_GENERATOR.value
        else:
            next_agent = NodeName.RESPONSE_FORMATTER.value

        self.logger.info(f"[Quiz Supervisor] → {next_agent}")
        return {**state, "next_agent": next_agent}

    def _question_generator_node(self, state: QuizState) -> Dict[str, Any]:
        self.logger.info("[Node] Running QuestionGenerator…")
        return self._question_generator.invoke(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Conditional Router
    # ══════════════════════════════════════════════════════════════════

    def _router(self, state: QuizState) -> str:
        """
        Reads ``state["next_agent"]`` set by the supervisor and returns
        the LangGraph node name (or END) for conditional edge routing.
        """
        next_agent = state.get("next_agent", "FINISH")

        if next_agent == "FINISH":
            self.logger.info("[Quiz Router] Workflow complete → END")
            return END

        valid_nodes = {
            NodeName.QUESTION_GENERATOR.value,
            NodeName.RESPONSE_FORMATTER.value,
        }

        if next_agent not in valid_nodes:
            self.logger.warning(
                f"[Quiz Router] Unknown next_agent='{next_agent}'. Terminating."
            )
            return END

        return next_agent

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Response Formatter
    # ══════════════════════════════════════════════════════════════════

    def _response_formatter_node(self, state: QuizState) -> Dict[str, Any]:
        """
        Builds the frontend-ready JSON response.

        The response includes all quiz questions with:
        - correct_answer labels
        - explanations
        - progressive hints (3 levels each)

        The backend server can then:
        - Serve questions to the frontend (hiding answers)
        - Evaluate submitted answers using the correct_answer data
        - Serve hints progressively based on hint_level
        """
        # ── Serialiser ────────────────────────────────────────────────
        def to_serializable(obj):
            """Recursively convert Pydantic models to plain dicts/lists."""
            if obj is None:
                return None
            if hasattr(obj, "model_dump"):
                return obj.model_dump()
            if hasattr(obj, "dict"):
                return obj.dict()
            if isinstance(obj, list):
                return [to_serializable(i) for i in obj]
            if isinstance(obj, dict):
                return {k: to_serializable(v) for k, v in obj.items()}
            return obj

        quiz_data = to_serializable(state.get("quiz_questions"))

        return {
            **state,
            "api_response": {
                "status":       "success",
                "mode":         "generate_quiz",
                "user_id":      state.get("user_id"),
                "career_track": state.get("career_track"),
                "stage_id":     state.get("stage_id"),
                "data": {
                    "quiz": quiz_data,
                },
                "error": state.get("error"),
            },
        }
