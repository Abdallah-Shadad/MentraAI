"""
TrackRecommenderGraph — Light Single-Agent Supervisor Graph
=============================================================
Recommends the best tech career tracks for a user based on their profile.

Architecture
------------
              ┌──────────────────┐
    START ───►│   SUPERVISOR     │
              │  (deterministic  │
              │   routing)       │
              └────────┬─────────┘
                       │
            ┌──────────▼──────────┐
            │  TrackRecommender   │
            │  (analyses profile, │
            │   returns top 3-5   │
            │   tech tracks)      │
            └──────────┬──────────┘
                       │
            ┌──────────▼──────────┐
            │  ResponseFormatter  │
            │  (builds API JSON)  │
            └──────────┬──────────┘
                       │
                      END

Flow
----
  START → supervisor → track_recommender
        → supervisor → response_formatter → END
"""

import logging
import json
from typing import Any, Dict, List, Optional, TypedDict, Annotated

from langchain_core.messages import BaseMessage
from langgraph.graph import StateGraph, END
from langgraph.graph.message import add_messages

from ..AgentEnums import AgentType
from ..AgentProviderFactory import AgentProviderFactory
from ...graph import GraphInterface
from ...graph.GraphEnums import GraphType, NodeName


# ══════════════════════════════════════════════════════════════════════════════
# 1. SHARED STATE
# ══════════════════════════════════════════════════════════════════════════════

class TrackRecommenderState(TypedDict, total=False):
    """
    Shared state dictionary that flows through every node in the
    TrackRecommenderGraph. All fields are optional (total=False).
    """

    # ── Inputs ─────────────────────────────────────────────────────────
    user_id: Annotated[str, "The unique identifier of the user"]
    profile: Annotated[
        Dict[str, Any],
        "User profile dict — may contain any subset of: "
        "Age, EdLevel, YearsCode, WorkExp, Employment, RemoteWork, "
        "Industry, OrgSize, AISelect, current_skills, future_skills",
    ]

    # ── TrackRecommender outputs ───────────────────────────────────────
    track_recommendations: Annotated[
        Any,
        "Structured recommendations produced by TrackRecommender agent.",
    ]
    track_recommender_done: Annotated[
        bool,
        "Signals completion of TrackRecommender. Used by Supervisor for routing.",
    ]

    # ── Supervisor control ─────────────────────────────────────────────
    next_agent: Annotated[str, "Next agent to route to, or 'FINISH'. Set by Supervisor."]
    error: Annotated[Optional[str], "Error message if any node fails."]

    # ── Message history (LangGraph convention) ─────────────────────────
    messages: Annotated[List[BaseMessage], add_messages]

    # ── Final formatted response ───────────────────────────────────────
    api_response: Annotated[
        Optional[Dict[str, Any]],
        "Frontend-ready JSON built by ResponseFormatter node.",
    ]


# ══════════════════════════════════════════════════════════════════════════════
# 2. TRACK RECOMMENDER GRAPH IMPLEMENTATION
# ══════════════════════════════════════════════════════════════════════════════

class TrackRecommenderGraph(GraphInterface):
    """
    Light single-agent graph for tech track recommendation.

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
        self._track_recommender = None

    # ── GraphInterface: identity ───────────────────────────────────────

    def get_graph_type(self) -> GraphType:
        return GraphType.TRACK_RECOMMENDER_GRAPH.value

    def get_state_schema(self) -> type:
        return TrackRecommenderState

    # ── GraphInterface: build & compile ────────────────────────────────

    def build(self) -> "TrackRecommenderGraph":
        """Wire all nodes, edges, and conditional routing."""
        self._setup_agents()

        workflow = StateGraph(TrackRecommenderState, name="Track Recommender System")

        # ── Register nodes ─────────────────────────────────────────────
        workflow.add_node(NodeName.SUPERVISOR.value,           self._supervisor_node)
        workflow.add_node(NodeName.TRACK_RECOMMENDER.value,    self._track_recommender_node)
        workflow.add_node(NodeName.RESPONSE_FORMATTER.value,   self._response_formatter_node)

        # ── Entry point ────────────────────────────────────────────────
        workflow.set_entry_point(NodeName.SUPERVISOR.value)

        # ── Conditional routing FROM supervisor ────────────────────────
        workflow.add_conditional_edges(
            NodeName.SUPERVISOR.value,
            self._router,
            {
                NodeName.TRACK_RECOMMENDER.value:   NodeName.TRACK_RECOMMENDER.value,
                NodeName.RESPONSE_FORMATTER.value:  NodeName.RESPONSE_FORMATTER.value,
                END:                                END,
            },
        )

        # ── TrackRecommender loops back to supervisor ──────────────────
        workflow.add_edge(NodeName.TRACK_RECOMMENDER.value, NodeName.SUPERVISOR.value)
        workflow.add_edge(NodeName.RESPONSE_FORMATTER.value, END)

        self._graph = workflow
        self.logger.info("[TrackRecommenderGraph] Graph built successfully.")
        return self

    def compile(self) -> Any:
        """Compile the StateGraph into a runnable LangGraph application."""
        if self._graph is None:
            raise RuntimeError("Call build() before compile().")
        self._app = self._graph.compile()
        self.logger.info("[TrackRecommenderGraph] Graph compiled successfully.")
        return self._app

    # ── GraphInterface: execution ──────────────────────────────────────

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """Run the full graph synchronously and return the final state."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before invoke().")
        self.logger.info(f"[TrackRecommenderGraph] Invoking for user: {state.get('user_id')}")
        return self._app.invoke(state)

    def stream(self, state: Dict[str, Any]):
        """Run the graph and yield step-by-step state updates."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before stream().")
        self.logger.info(f"[TrackRecommenderGraph] Streaming for user: {state.get('user_id')}")
        yield from self._app.stream(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Agent Setup
    # ══════════════════════════════════════════════════════════════════

    def _setup_agents(self):
        """Instantiate the TrackRecommender via AgentProviderFactory."""
        if self.agent_factory is None:
            raise RuntimeError(
                "TrackRecommenderGraph requires an AgentProviderFactory. "
                "Pass agent_factory= when constructing TrackRecommenderGraph."
            )

        self._track_recommender = self.agent_factory.create(
            AgentType.TRACK_RECOMMENDER, llm=self.llm
        )

        self.logger.info("[TrackRecommenderGraph] TrackRecommender agent initialised.")

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Graph Nodes
    # ══════════════════════════════════════════════════════════════════

    def _supervisor_node(self, state: TrackRecommenderState) -> Dict[str, Any]:
        """
        Deterministic supervisor — routes to TrackRecommender first,
        then to ResponseFormatter once recommendations are generated.
        """
        self.logger.info("[TrackRecommender Supervisor] Deciding next agent...")

        if state.get("error"):
            self.logger.error(f"[TrackRecommender Supervisor] Found error in state: {state.get('error')}. Routing to ResponseFormatter to prevent infinite loop.")
            next_agent = NodeName.RESPONSE_FORMATTER.value
        elif not state.get("track_recommender_done"):
            next_agent = NodeName.TRACK_RECOMMENDER.value
        else:
            next_agent = NodeName.RESPONSE_FORMATTER.value

        self.logger.info(f"[TrackRecommender Supervisor] → {next_agent}")
        return {**state, "next_agent": next_agent}

    def _track_recommender_node(self, state: TrackRecommenderState) -> Dict[str, Any]:
        self.logger.info("[Node] Running TrackRecommender…")
        return self._track_recommender.invoke(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Conditional Router
    # ══════════════════════════════════════════════════════════════════

    def _router(self, state: TrackRecommenderState) -> str:
        """
        Reads ``state["next_agent"]`` set by the supervisor and returns
        the LangGraph node name (or END) for conditional edge routing.
        """
        next_agent = state.get("next_agent", "FINISH")

        if next_agent == "FINISH":
            self.logger.info("[TrackRecommender Router] Workflow complete → END")
            return END

        valid_nodes = {
            NodeName.TRACK_RECOMMENDER.value,
            NodeName.RESPONSE_FORMATTER.value,
        }

        if next_agent not in valid_nodes:
            self.logger.warning(
                f"[TrackRecommender Router] Unknown next_agent='{next_agent}'. Terminating."
            )
            return END

        return next_agent

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Response Formatter
    # ══════════════════════════════════════════════════════════════════

    def _response_formatter_node(self, state: TrackRecommenderState) -> Dict[str, Any]:
        """
        Builds the frontend-ready JSON response from the
        TrackRecommender agent's structured output.
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

        state_error = state.get("error")
        recommendations_data = to_serializable(state.get("track_recommendations"))

        api_status = "error" if (state_error or recommendations_data is None) else "success"

        return {
            **state,
            "api_response": {
                "status":       api_status,
                "mode":         "track_recommendation",
                "user_id":      state.get("user_id"),
                "data": {
                    "recommendations": recommendations_data,
                },
                "error": state_error,
            },
        }
