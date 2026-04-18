"""
RoadmapGraph — Multi-Agent Supervisor Graph
============================================
Implements the Roadmap Supervisor pattern using LangGraph.

Architecture
------------
                    ┌──────────────────┐
          START ───►│   SUPERVISOR     │
                    │  (decides next   │
                    │   agent to call) │
                    └────────┬─────────┘
                             │  conditional routing
              ┌──────────────┼──────────────────┐
              ▼              ▼                  ▼
    ┌─────────────────┐  ┌──────────────────┐  ┌──────────────────┐
    │ ProfileAnalyzer │  │CurriculumGenerator│  │ ResourceCurator  │
    └────────┬────────┘  └────────┬─────────┘  └────────┬─────────┘
             └───────────────────►│◄───────────────────┘
                                  │
                          ┌───────▼──────────┐
                          │ AdaptationEngine  │ (optional, if progress data)
                          └───────┬──────────┘
                                  │
                                 END


Supervisor Logic
----------------
The supervisor inspects the shared state after each agent finishes and
routes execution to the next appropriate agent, or terminates with END.

Sequential happy-path flow:
  START → supervisor → profile_analyzer
        → supervisor → curriculum_generator
        → supervisor → resource_curator
        → supervisor → stage_extractor
        → supervisor → [adaptation_engine if learner_progress]
        → supervisor → response_formatter
        → response_formatter → END
"""

import logging
import json
from typing import Any, Dict, List, Literal, Optional, TypedDict, Annotated

from langchain_core.messages import BaseMessage, SystemMessage, HumanMessage, AIMessage
from langchain_core.tools import BaseTool
from langgraph.graph import StateGraph, END, START
from langgraph.graph.message import add_messages

from ..AgentEnums import AgentType
from ..AgentProviderFactory import AgentProviderFactory
from ...graph import GraphInterface
from ...graph.GraphEnums import GraphType, NodeName
from .agents.schemas.AdaptationEngine import Resource

# ══════════════════════════════════════════════════════════════════════════════
# 1. SHARED STATE
# ══════════════════════════════════════════════════════════════════════════════

class RoadmapState(TypedDict, total=False):
    """
    Shared state dictionary that flows through every node in the
    RoadmapGraph. All fields are optional (total=False) so individual
    agents only need to write the keys they produce.
    """

    # ── Inputs ─────────────────────────────────────────────────────────
    user_id: Annotated[str, "The unique identifier of the user (e.g., 'user_123'). Provided as initial input."]
    career_track: Annotated[str, "The user's chosen career track (e.g., 'Data Science', 'Web Development'). Provided as initial input."]
    weekly_hours: Annotated[int, "Number of hours the user can dedicate to learning per week (e.g., 10). Provided as initial input."]
    user_background :Annotated[str,"The user's background information (e.g., 'I have a degree in computer science and 2 years of experience in software development'). Provided as initial input."]
    current_skills :Annotated[List[str],"The user's current skills (e.g., ['Python', 'Machine Learning']). Provided as initial input."]

    # ── Profile Analyzer outputs ────────────────────────────────────────
    difficulty_level: Annotated[str, "Assessment of current competency: 'beginner', 'intermediate', or 'advanced'. Produced by Profile Analyzer."]
    skill_gaps: Annotated[List[str], "Areas needing improvement compared to career track requirements. Example: ['Machine Learning']. Produced by Profile Analyzer."]
    prerequisite_analysis: Annotated[Dict[str, Any], "Details of existing foundations vs needed foundations based on the user's background. Produced by Profile Analyzer."]
    estimated_duration_weeks: Annotated[int, "Estimated total time required in weeks based on weekly availability. Produced by Profile Analyzer."]
    profile_agent_done: Annotated[bool, "Signals completion of Profile Analyzer execution. Used by Supervisor for routing."]
    ProfileAnalyzer_Summary: Annotated[str, "Summary of the user. Produced by Profile Analyzer."]
    
    # ── Curriculum Generator outputs ────────────────────────────────────
    curriculum: Annotated[Any, "Detailed curriculum plan containing 'stages' (list of stage objects with objectives and weeks) and 'dependencies'. Produced by Curriculum Generator."]
    curriculum_agent_done: Annotated[bool, "Signals completion of Curriculum Generator execution. Used by Supervisor for routing."]

    # ── Resource Curator outputs ────────────────────────────────────────
    resources: Annotated[Any, "High quality learning materials curated for each stage. Format: dict mapping stage_id to a list of resource dicts. Produced by Resource Curator."]
    resource_agent_done: Annotated[bool, "Signals completion of Resource Curator execution. Used by Supervisor for routing."]

    # ── Adaptation Engine outputs (optional / triggered by progress) ─────
    struggling_topics: Annotated[Optional[List[str]], "List of topics the user is struggling with. that get from user feedback and quiz attempts. Produced by Adaptation Engine."]
    adapted_recommended_resources: Annotated[Optional[List[Resource]], "List of recommended resources based on user's struggling topics. Produced by Adaptation Engine."]
    adaptation_agent_done: Annotated[bool, "Signals completion of Adaptation Engine execution. Used by Supervisor for routing."]
    adaptation_summary: Annotated[str, "Summary of the adaptation. Produced by Adaptation Engine."]

    # ── Supervisor control ──────────────────────────────────────────────
    next_agent: Annotated[str, "Name of the next agent to route to, or 'FINISH'. Set by Supervisor, read by Router."]
    workflow_complete: Annotated[bool, "Signals if the entire roadmap has been finalized by the Supervisor."]
    error: Annotated[Optional[str], "Used for error tracking and reporting if any node fails during execution."]

    # ── Message history (LangGraph convention) ──────────────────────────
    messages: Annotated[List[BaseMessage], add_messages]

    # ── Stage progression control ───────────────────────────────────────
    is_stage_progression: Annotated[bool, "False = initial build, True = advance to next stage. Controls workflow mode."]
    all_stages: Annotated[List[Dict], "Extracted from curriculum, flat list of stage objects. Used by Stage Extractor."]
    current_stage_index: Annotated[int, "0-based index of the stage to process right now. Used by Stage Extractor."]
    current_stage: Annotated[Optional[Dict], "Single stage object being processed currently, containing 'id' and 'topics'. Extracted by Stage Extractor."]
    stage_extractor_done: Annotated[bool, "Signals completion of Stage Extractor execution. Used by Supervisor for routing."]
    stage_resources: Annotated[Optional[Any], "Array of curated resources for current_stage ONLY. Produced by Resource Curator."]
    completed_stages: Annotated[List[str], "List of stage IDs the user has successfully finished."]


# ══════════════════════════════════════════════════════════════════════════════
# 2. ROADMAP GRAPH IMPLEMENTATION
# ══════════════════════════════════════════════════════════════════════════════

class RoadmapGraph(GraphInterface):
    """
    Roadmap Multi-Agent Supervisor Graph.

    Implements ``GraphInterface`` — build once, compile, then invoke or stream.

    Parameters
    ----------
    config        :  Project-wide config / settings object.
    agent_factory :  ``AgentProviderFactory`` that provides all worker agents.
    llm           :  An LLMInterface-compatible provider for the supervisor node.
    """

    # ── System prompt for the supervisor node ──────────────────────────
    _SUPERVISOR_PROMPT = """You are the Roadmap Supervisor, orchestrating a team of AI agents
to build a personalised learning roadmap for a user.

You must decide which agent to invoke next based on the current state.

Available agents and when to call them:
- "profile_analyzer"     : Always first. Analyses user background & skill gaps.
- "curriculum_generator" : After profile analysis. Builds stage-by-stage curriculum.
- "stage_extractor"      : After curriculum. Extracts the single current stage from all_stages by index.
- "resource_curator"     : After stage_extractor. Finds learning materials for each stage.
- "adaptation_engine"    : Only if learner_progress data exists. Adapts the roadmap.
- "response_formatter"   : Always last. Builds the frontend-ready JSON.
- "FINISH"               : When the roadmap is complete (all required agents done).

Rules:
1. Always start with profile_analyzer if profile_agent_done is false/missing.
2. Move to curriculum_generator once profile_agent_done is true.
3. Move to stage_extractor once curriculum_agent_done is true AND stage_extractor_done is false.
4. Move to resource_curator once stage_extractor_done is true.
5. Move to adaptation_engine ONLY if learner_progress is provided AND adaptation_agent_done is false.
6. Move to response_formatter once resource_agent_done is true.
7. Set "FINISH" when response_formatter is true (and adaptation is done or not needed).
8. repeat an agent if quality of output is not Enough and need more Resources and quality.

Respond with ONLY a JSON object:
{"next_agent": "<agent_name_or_FINISH>", "reason": "<one line explanation>"}
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

        # Worker agents — created from agent_factory in _setup_agents()
        self._profile_analyzer = None
        self._curriculum_generator = None
        self._stage_extractor = None
        self._resource_curator = None
        self._adaptation_engine = None
        self._response_formatter = None

    # ── GraphInterface: identity ───────────────────────────────────────

    def get_graph_type(self) -> GraphType:
        return GraphType.ROADMAP_GRAPH.value

    def get_state_schema(self) -> type:
        return RoadmapState

    # ── GraphInterface: build & compile ────────────────────────────────

    def build(self) -> "RoadmapGraph":
        """Wire all nodes, edges, and conditional routing."""
        self._setup_agents()

        workflow = StateGraph(RoadmapState, name="Roadmap Multi-Agent System")

        # ── Register nodes ─────────────────────────────────────────────
        workflow.add_node(NodeName.SUPERVISOR.value,         self._supervisor_node)
        workflow.add_node(NodeName.PROFILE_ANALYZER.value,  self._profile_analyzer_node)
        workflow.add_node(NodeName.CURRICULUM_GENERATOR.value, self._curriculum_generator_node)
        workflow.add_node(NodeName.STAGE_EXTRACTOR.value,   self._stage_extractor_node)
        workflow.add_node(NodeName.RESOURCE_CURATOR.value,  self._resource_curator_node)
        workflow.add_node(NodeName.ADAPTATION_ENGINE.value, self._adaptation_engine_node)
        workflow.add_node(NodeName.RESPONSE_FORMATTER.value, self._response_formatter_node)

        # ── Entry point ────────────────────────────────────────────────
        workflow.set_entry_point(NodeName.SUPERVISOR.value)

        # ── Conditional routing FROM supervisor ────────────────────────
        workflow.add_conditional_edges(
            NodeName.SUPERVISOR.value,
            self._router,
            {
                NodeName.PROFILE_ANALYZER.value:     NodeName.PROFILE_ANALYZER.value,
                NodeName.CURRICULUM_GENERATOR.value: NodeName.CURRICULUM_GENERATOR.value,
                NodeName.STAGE_EXTRACTOR.value:      NodeName.STAGE_EXTRACTOR.value,
                NodeName.RESOURCE_CURATOR.value:     NodeName.RESOURCE_CURATOR.value,
                NodeName.ADAPTATION_ENGINE.value:    NodeName.ADAPTATION_ENGINE.value,
                NodeName.RESPONSE_FORMATTER.value:   NodeName.RESPONSE_FORMATTER.value,
                END:                                 END,
            },
        )

        # ── All worker agents loop back to supervisor ──────────────────
        for node in [
            NodeName.PROFILE_ANALYZER.value,
            NodeName.CURRICULUM_GENERATOR.value,
            NodeName.STAGE_EXTRACTOR.value,
            NodeName.RESOURCE_CURATOR.value,
            NodeName.ADAPTATION_ENGINE.value,
        ]:
            workflow.add_edge(node, NodeName.SUPERVISOR.value)

        workflow.add_edge(NodeName.RESPONSE_FORMATTER.value, END)
        
        self._graph = workflow
        self.logger.info("[RoadmapGraph] Graph built successfully.")
        return self

    def compile(self) -> Any:
        """Compile the StateGraph into a runnable LangGraph application."""
        if self._graph is None:
            raise RuntimeError("Call build() before compile().")
        self._app = self._graph.compile()
        self.logger.info("[RoadmapGraph] Graph compiled successfully.")
        return self._app

    # ── GraphInterface: execution ──────────────────────────────────────

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """Run the full graph synchronously and return the final state."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before invoke().")
        self.logger.info(f"[RoadmapGraph] Invoking for user: {state.get('user_id')}")
        return self._app.invoke(state)

    def stream(self, state: Dict[str, Any]):
        """Run the graph and yield step-by-step state updates."""
        if self._app is None:
            raise RuntimeError("Call build().compile() before stream().")
        self.logger.info(f"[RoadmapGraph] Streaming for user: {state.get('user_id')}")
        yield from self._app.stream(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Agent Setup
    # ══════════════════════════════════════════════════════════════════

    def _setup_agents(self):
        """Instantiate all worker agents via the AgentProviderFactory."""
        if self.agent_factory is None:
            raise RuntimeError(
                "RoadmapGraph requires an AgentProviderFactory. "
                "Pass agent_factory= when constructing RoadmapGraph."
            )

        self._profile_analyzer     = self.agent_factory.create(AgentType.PROFILE_ANALYZER,     llm=self.llm)
        self._curriculum_generator = self.agent_factory.create(AgentType.CURRICULUM_GENERATOR, llm=self.llm)
        self._resource_curator     = self.agent_factory.create(AgentType.RESOURCE_CURATOR,     llm=self.llm)
        self._adaptation_engine    = self.agent_factory.create(AgentType.ADAPTATION_ENGINE,    llm=self.llm)
        # NOTE: stage_extractor and response_formatter are plain node methods on this
        # class — they are NOT agents and do NOT need to be created via agent_factory.

        self.logger.info("[RoadmapGraph] All agents initialised.")

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Graph Nodes
    # ══════════════════════════════════════════════════════════════════

    def _supervisor_node(self, state: RoadmapState) -> Dict[str, Any]:
        """
        Supervisor decides which worker agent to invoke next.
        Uses the LLM to reason about current state and return next_agent.
        """
        self.logger.info("[Supervisor] Deciding next agent...")

        if self.llm is None:
            # Fallback: deterministic rule-based routing (no LLM needed)
            return self._deterministic_routing(state)

        # Build a compact state summary for the LLM
        state_summary = {
            "profile_agent_done":      state.get("profile_agent_done", False),
            "curriculum_agent_done":   state.get("curriculum_agent_done", False),
            "stage_extractor_done":    state.get("stage_extractor_done", False),  # FIX: was missing — caused infinite loop
            "resource_agent_done":     state.get("resource_agent_done", False),
            "adaptation_agent_done":   state.get("adaptation_agent_done", False),
            "has_learner_progress":    bool(state.get("learner_progress")),
            "workflow_complete":       state.get("workflow_complete", False),
        }

        messages = [
            SystemMessage(content=self._SUPERVISOR_PROMPT),
            HumanMessage(content=f"Current state:\n{json.dumps(state_summary, indent=2)}\n\nWhat should we do next?"),
        ]

        try:
            from pydantic import BaseModel, Field
            class SupervisorRoutingOutput(BaseModel):
                next_agent: str = Field(description="Name of the next agent to route to, or 'FINISH'")
                reason: str = Field(description="A short explanation for this routing decision")

            # Call with_structured_output on the raw LLM client with a Pydantic schema
            structured_chain = self.llm.client.with_structured_output(SupervisorRoutingOutput)
            parsed = structured_chain.invoke(messages)
            
            # structured_chain with a BaseModel returns an instance of that model, not a dict
            next_agent = parsed.next_agent
            reason     = parsed.reason
            self.logger.info(f"[Supervisor] → {next_agent}  ({reason})")

            return {
                **state,
                "next_agent": next_agent,
                "messages": [AIMessage(content=f"Routing to: {next_agent}. Reason: {reason}")],
            }

        except Exception as e:
            self.logger.error(f"[Supervisor] LLM routing failed: {e}. Falling back to deterministic routing.")
            return self._deterministic_routing(state)

    def _deterministic_routing(self, state: RoadmapState) -> Dict[str, Any]:
        """
        Fallback supervisor logic — pure rule-based, no LLM required.
        This guarantees correct routing even if the LLM is not set.
        """
        is_progression = state.get("is_stage_progression", False)

        if not is_progression:
            # ── MODE 1: Initial build ──────────────────────────────
            if not state.get("profile_agent_done"):
                next_agent = NodeName.PROFILE_ANALYZER.value
            elif not state.get("curriculum_agent_done"):
                next_agent = NodeName.CURRICULUM_GENERATOR.value
            else:
                # Curriculum is ready — stop here, return roadmap to user
                next_agent = "FINISH"
        else:
            # ── MODE 2: Stage progression ──────────────────────────
            if not state.get("curriculum_agent_done"):
                next_agent = NodeName.CURRICULUM_GENERATOR.value
            elif not state.get("stage_extractor_done"):          # FIX: was skipped entirely
                next_agent = NodeName.STAGE_EXTRACTOR.value
            elif state.get("learner_progress") and not state.get("adaptation_agent_done"):
                next_agent = NodeName.ADAPTATION_ENGINE.value
            elif not state.get("resource_agent_done"):
                next_agent = NodeName.RESOURCE_CURATOR.value
            else:
                next_agent = "FINISH"

        return {**state, "next_agent": next_agent}

    # ── Worker node wrappers ────────────────────────────────────────────

    def _profile_analyzer_node(self, state: RoadmapState) -> Dict[str, Any]:
        self.logger.info("[Node] Running ProfileAnalyzer…")
        return self._profile_analyzer.invoke(state)

    def _curriculum_generator_node(self, state: RoadmapState) -> Dict[str, Any]:
        self.logger.info("[Node] Running CurriculumGenerator…")
        return self._curriculum_generator.invoke(state)

    def _resource_curator_node(self, state: RoadmapState) -> Dict[str, Any]:
        self.logger.info("[Node] Running ResourceCurator…")
        return self._resource_curator.invoke(state)

    def _adaptation_engine_node(self, state: RoadmapState) -> Dict[str, Any]:
        self.logger.info("[Node] Running AdaptationEngine…")
        return self._adaptation_engine.invoke(state)

    # ══════════════════════════════════════════════════════════════════
    # PRIVATE — Conditional Router
    # ══════════════════════════════════════════════════════════════════

    def _router(self, state: RoadmapState) -> str:
        """
        Reads ``state["next_agent"]`` set by the supervisor and returns
        the LangGraph node name (or END) for conditional edge routing.
        """
        next_agent = state.get("next_agent", "FINISH")

        if next_agent == "FINISH":
            self.logger.info("[Router] Workflow complete → END")
            return END

        # Validate the returned node name exists
        valid_nodes = {
            NodeName.PROFILE_ANALYZER.value,
            NodeName.CURRICULUM_GENERATOR.value,
            NodeName.STAGE_EXTRACTOR.value,
            NodeName.RESOURCE_CURATOR.value,
            NodeName.ADAPTATION_ENGINE.value,
            NodeName.RESPONSE_FORMATTER.value,
        }

        if next_agent not in valid_nodes:
            self.logger.warning(
                f"[Router] Unknown next_agent='{next_agent}'. Terminating."
            )
            return END

        return next_agent


    def _stage_extractor_node(self, state: RoadmapState) -> Dict[str, Any]:
        """Extracts the single current stage from all_stages by index."""
        self.logger.info("[Node] Running StageExtractor…")
        all_stages = state.get("all_stages", [])
        idx = state.get("current_stage_index", 0)

        if not all_stages:
            # Parse from curriculum if not already extracted
            curriculum = state.get("curriculum", {})
            if isinstance(curriculum, str):
                import json
                curriculum = json.loads(curriculum)
                
            if isinstance(curriculum, dict):
                all_stages = curriculum.get("stages", [])
            else:
                # Handle Pydantic objects (CurriculumOutput) — convert to plain dicts
                raw_stages = getattr(curriculum, "stages", [])
                all_stages = [
                    s.model_dump() if hasattr(s, "model_dump")
                    else s.dict() if hasattr(s, "dict")
                    else s
                    for s in raw_stages
                ]

        current_stage = all_stages[idx] if idx < len(all_stages) else None

        # current_stage may be a Pydantic model or a plain dict — handle both
        if current_stage is not None:
            if hasattr(current_stage, "model_dump"):          # Pydantic v2
                current_stage = current_stage.model_dump()
            elif hasattr(current_stage, "dict"):              # Pydantic v1
                current_stage = current_stage.dict()
            # else: already a plain dict — no conversion needed

        self.logger.info(f"[StageExtractor] Current stage index {idx}: {current_stage.get('name') if current_stage else 'None'}")

        return {
            **state,
            "all_stages": all_stages,
            "current_stage": current_stage,
            "stage_extractor_done": True,   # FIX: signal to supervisor that extraction is complete
            "resource_agent_done": False,   # reset so ResourceCurator runs fresh
        }

    def _response_formatter_node(self, state: RoadmapState) -> Dict[str, Any]:
        """Always last node before END. Builds the frontend-ready JSON."""
        mode = "stage_resources" if state.get("is_stage_progression") else "roadmap_overview"

        def to_serializable(obj):
            """Recursively convert Pydantic models and other non-serializable objects to dicts."""
            if obj is None:
                return None
            if hasattr(obj, "model_dump"):   # Pydantic v2
                return obj.model_dump()
            if hasattr(obj, "dict"):         # Pydantic v1
                return obj.dict()
            if isinstance(obj, list):
                return [to_serializable(i) for i in obj]
            if isinstance(obj, dict):
                return {k: to_serializable(v) for k, v in obj.items()}
            return obj

        return {
            **state,
            "api_response": {
                "status": "success",
                "mode": mode,
                "user_id": state.get("user_id"),
                "career_track": state.get("career_track"),
                "data": {
                    # Mode 1 — initial build
                    "curriculum":       to_serializable(state.get("curriculum")),
                    "difficulty_level": state.get("difficulty_level"),
                    "skill_gaps":       state.get("skill_gaps"),
                    "total_weeks":      state.get("estimated_duration_weeks"),

                    # Mode 2 — stage progression
                    "current_stage":    to_serializable(state.get("current_stage")),
                    "stage_index":      state.get("current_stage_index"),
                    "stage_resources":  to_serializable(state.get("stage_resources")),
                    "adapted":          bool(state.get("adapted_curriculum")),
                },
                "error": state.get("error"),
            }
        }