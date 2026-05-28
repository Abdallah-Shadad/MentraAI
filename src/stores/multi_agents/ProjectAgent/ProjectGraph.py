"""
ProjectGraph — Multi-Agent Supervisor Graph (Project Recommendations)
======================================================================
Implements the Project Supervisor pattern using LangGraph.

This graph is responsible for generating job-market-relevant project
recommendations after a learner completes a stage, or for designing
multi-stage capstone projects.

Architecture
------------
                  ┌──────────────────┐
        START ───►│   SUPERVISOR     │
                  │  (routes to      │
                  │   next agent)    │
                  └────────┬─────────┘
                           │
                ┌──────────▼──────────┐
                │ ProjectRecommender  │
                │ (generates project  │
                │  recommendations    │
                │  with milestones)   │
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
  START → supervisor → project_recommender
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

class ProjectState(TypedDict, total=False):
    """
    Shared state dictionary that flows through every node in the
    ProjectGraph. All fields are optional (total=False) so individual
    agents only need to write the keys they produce.
    """

    # ── Inputs ─────────────────────────────────────────────────────────
    user_id: Annotated[str, "The unique identifier of the user"]
    career_track: Annotated[str, "The user's chosen career track"]
    stage_id: Annotated[str, "The curriculum stage to recommend projects for"]
    stage_name: Annotated[str, "Human-readable name of the stage"]
    topics: Annotated[List[str], "Topics covered in this stage"]
    learning_objectives: Annotated[Any, "Learning objectives for this stage (dict or list)"]
    difficulty_level: Annotated[str, "Stage difficulty: 'beginner', 'intermediate', 'advanced'"]
    completed_stages: Annotated[List[Any], "List of already-completed stage objects or IDs"]
    upcoming_stages: Annotated[List[Any], "List of upcoming stage objects or IDs"]

    # ── ProjectRecommender outputs ──────────────────────────────────────
    project_recommendations: Annotated[Any, "The generated project recommendations with milestones. Produced by ProjectRecommender."]
    project_recommender_done: Annotated[bool, "Signals completion of ProjectRecommender. Used by Supervisor for routing."]

    # ── Supervisor control ──────────────────────────────────────────────
    next_agent: Annotated[str, "Next agent to route to, or 'FINISH'. Set by Supervisor."]
    error: Annotated[Optional[str], "Error message if any node fails."]

    # ── Message history (LangGraph convention) ──────────────────────────
    messages: Annotated[List[BaseMessage], add_messages]

    # ── Final formatted response ────────────────────────────────────────
    api_response: Annotated[Optional[Dict[str, Any]], "Frontend-ready JSON built by ResponseFormatter node."]


# ══════════════════════════════════════════════════════════════════════════════
# 2. PROJECT GRAPH IMPLEMENTATION
# ══════════════════════════════════════════════════════════════════════════════

class ProjectGraph(GraphInterface):
    """
    Project Multi-Agent Supervisor Graph — Project Recommendations.

    The AI server generates job-market-aligned project recommendations
    with milestones mapped to curriculum stages.

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
        self._project_recommender = None

    # ── GraphInterface: identity ───────────────────────────────────────

    def get_graph_type(self) -> GraphType:
        return GraphType.PROJECT_GRAPH.value

    def get_state_schema(self) -> type:
        return ProjectState

    # ── GraphInterface: build & compile ────────────────────────────────

    def build(self) -> "ProjectGraph":
        """Wire all nodes, edges, and conditional routing."""
        self._setup_agents()

        workflow = StateGraph(ProjectState, name="Project Multi-Agent System")

        # ── Register nodes ─────────────────────────────────────────────
        workflow.add_node(NodeName.SUPERVISOR.value,            self._supervisor_node)
        workflow.add_node(NodeName.PROJECT_RECOMMENDER.value,   self._project_recommender_node)
        workflow.add_node(NodeName.RESPONSE_FORMATTER.value,    self._response_formatter_node)

        # ── Entry point ────────────────────────────────────────────────
        workflow.set_entry_point(NodeName.SUPERVISOR.value)

        # ── Conditional routing FROM supervisor ────────────────────────
        workflow.add_conditional_edges(
            NodeName.SUPERVISOR.value,
            self._router,
            {
                NodeName.PROJECT_RECOMMENDER.value: NodeName.PROJECT_RECOMMENDER.value,
                NodeName.RESPONSE_FORMATTER.value:  NodeName.RESPONSE_FORMATTER.value,
                END:                                END,
            },
        )

        # ── ProjectRecommender loops back to supervisor ────────────────
        workflow.add_edge(NodeName.PROJECT_RECOMMENDER.value, NodeName.SUPERVISOR.value)
        workflow.add_edge(NodeName.RESPONSE_FORMATTER.value, END)

        self._graph = workflow
        self.logger.info("[ProjectGraph] Graph built successfully.")
        return self

    def compile(self) -> Any:
        """Compile the StateGraph into a runnable LangGraph application."""
        if self._graph is None:
            raise RuntimeError("Call build() before compile().")
        self._app = self._graph.compile()
        self.logger.info("[ProjectGraph] Graph compiled successfully.")
        return self._app

    # ── GraphInterface: execution ──────────────────────────────────────

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """Run the full graph synchronously and return the final state."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before invoke().")
        self.logger.info(f"[ProjectGraph] Invoking for user: {state.get('user_id')}")
        return self._app.invoke(state)

    def stream(self, state: Dict[str, Any]):
        """Run the graph and yield step-by-step state updates."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before stream().")
        self.logger.info(f"[ProjectGraph] Streaming for user: {state.get('user_id')}")
        yield from self._app.stream(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Agent Setup
    # ══════════════════════════════════════════════════════════════════

    def _setup_agents(self):
        """Instantiate the ProjectRecommender via AgentProviderFactory."""
        if self.agent_factory is None:
            raise RuntimeError(
                "ProjectGraph requires an AgentProviderFactory. "
                "Pass agent_factory= when constructing ProjectGraph."
            )

        self._project_recommender = self.agent_factory.create(
            AgentType.PROJECT_RECOMMENDER, llm=self.llm
        )

        self.logger.info("[ProjectGraph] ProjectRecommender agent initialised.")

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Graph Nodes
    # ══════════════════════════════════════════════════════════════════

    def _supervisor_node(self, state: ProjectState) -> Dict[str, Any]:
        """
        Deterministic supervisor — routes to ProjectRecommender first,
        then to ResponseFormatter once recommendations are generated.
        """
        self.logger.info("[Project Supervisor] Deciding next agent...")

        if not state.get("project_recommender_done"):
            next_agent = NodeName.PROJECT_RECOMMENDER.value
        else:
            next_agent = NodeName.RESPONSE_FORMATTER.value

        self.logger.info(f"[Project Supervisor] → {next_agent}")
        return {**state, "next_agent": next_agent}

    def _project_recommender_node(self, state: ProjectState) -> Dict[str, Any]:
        self.logger.info("[Node] Running ProjectRecommender…")
        return self._project_recommender.invoke(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Conditional Router
    # ══════════════════════════════════════════════════════════════════

    def _router(self, state: ProjectState) -> str:
        """
        Reads ``state["next_agent"]`` set by the supervisor and returns
        the LangGraph node name (or END) for conditional edge routing.
        """
        next_agent = state.get("next_agent", "FINISH")

        if next_agent == "FINISH":
            self.logger.info("[Project Router] Workflow complete → END")
            return END

        valid_nodes = {
            NodeName.PROJECT_RECOMMENDER.value,
            NodeName.RESPONSE_FORMATTER.value,
        }

        if next_agent not in valid_nodes:
            self.logger.warning(
                f"[Project Router] Unknown next_agent='{next_agent}'. Terminating."
            )
            return END

        return next_agent

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Response Formatter
    # ══════════════════════════════════════════════════════════════════

    def _response_formatter_node(self, state: ProjectState) -> Dict[str, Any]:
        """
        Builds the frontend-ready JSON response.

        The response includes all project recommendations with:
        - milestones mapped to curriculum stages
        - market relevance descriptions
        - portfolio tips
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

        project_data = to_serializable(state.get("project_recommendations"))

        return {
            **state,
            "api_response": {
                "status":       "success",
                "mode":         "project_recommendations",
                "user_id":      state.get("user_id"),
                "career_track": state.get("career_track"),
                "stage_id":     state.get("stage_id"),
                "data": {
                    "recommendations": project_data,
                },
                "error": state.get("error"),
            },
        }
