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
from .agents.schemas.AdaptationEngine import RemediationResource

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
    adapted_curriculum: Annotated[Optional[Any], "Full AdaptationEngineOutput containing remedial resources, stage adjustments, etc."]
    struggling_topics: Annotated[Optional[List[str]], "List of topics the user is struggling with. that get from user feedback and quiz attempts. Produced by Adaptation Engine."]
    adapted_recommended_resources: Annotated[Optional[List[RemediationResource]], "List of recommended resources based on user's struggling topics. Produced by Adaptation Engine."]
    adaptation_agent_done: Annotated[bool, "Signals completion of Adaptation Engine execution. Used by Supervisor for routing."]
    adaptation_summary: Annotated[str, "Summary of the adaptation. Produced by Adaptation Engine."]

    # ── Quiz / Adaptation-mode inputs ───────────────────────────────────
    stage_id: Annotated[Optional[str], "ID of the stage the learner just attempted (e.g. 'stage_0'). Used in adaptation mode."]
    stage_name: Annotated[Optional[str], "Name of the stage the learner just attempted. Used in adaptation mode."]
    is_adaptation_mode: Annotated[bool, "When True the Supervisor skips full pipeline and routes only to AdaptationEngine → ResponseFormatter."]

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

    # ── Final formatted response (written by ResponseFormatter) ────────
    api_response: Annotated[Optional[Dict[str, Any]], "Frontend-ready JSON built by ResponseFormatter node. This is what the API endpoint returns."]


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

## ADAPTATION MODE (is_adaptation_mode = true)
When `is_adaptation_mode` is true, the learner has FAILED a stage quiz (score < 50%).
In this case you MUST follow this short pipeline ONLY:
  1. Route to "adaptation_engine"  (if adaptation_agent_done is false)
  2. Route to "response_formatter" (once adaptation_agent_done is true)
  3. Set "FINISH" after response_formatter completes.

## INITIAL BUILD MODE (is_stage_progression = false)
When building the roadmap for the first time:
1. Always start with "profile_analyzer" if profile_agent_done is false/missing.
2. Move to "curriculum_generator" once profile_agent_done is true AND curriculum_agent_done is false.
3. Move to "response_formatter" once curriculum_agent_done is true.

## STAGE PROGRESSION MODE (is_stage_progression = true)
When the user is advancing to the next stage and needs resources:
1. Call "curriculum_generator" if curriculum_agent_done is false (to load the curriculum).
2. Move to "stage_extractor" once curriculum_agent_done is true AND stage_extractor_done is false.
3. Move to "adaptation_engine" IF has_learner_progress is true AND adaptation_agent_done is false. DO NOT move to resource_curator until this is done.
4. Move to "resource_curator" ONLY IF stage_extractor_done is true AND (has_learner_progress is false OR adaptation_agent_done is true).
5. Move to "response_formatter" once resource_agent_done is true.
6. Set "FINISH" when response_formatter is done.

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

        # ── Prevent Infinite Loops ──
        if state.get("error"):
            self.logger.error(f"[Supervisor] Found error in state: {state.get('error')}. Routing to FINISH to prevent infinite loop.")
            return {**state, "next_agent": "FINISH"}

        if self.llm is None:
            # Fallback: deterministic rule-based routing (no LLM needed)
            return self._deterministic_routing(state)

        # Build a compact state summary for the LLM
        state_summary = {
            "is_stage_progression":    state.get("is_stage_progression", False),
            "is_adaptation_mode":      state.get("is_adaptation_mode", False),
            "profile_agent_done":      state.get("profile_agent_done", False),
            "curriculum_agent_done":   state.get("curriculum_agent_done", False),
            "stage_extractor_done":    state.get("stage_extractor_done", False),
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

        Mode 0 — Adaptation only  (is_adaptation_mode=True)
        ─────────────────────────────────────────────────────
        Learner failed a quiz (score < 50 %).  Skip the full pipeline;
        route straight to AdaptationEngine then ResponseFormatter.

        Mode 1 — Initial roadmap build  (is_stage_progression=False)
        ───────────────────────────────────────────────────────────────
        ProfileAnalyzer → CurriculumGenerator → FINISH

        Mode 2 — Stage progression  (is_stage_progression=True)
        ──────────────────────────────────────────────────────────
        CurriculumGenerator → StageExtractor → [AdaptationEngine] → ResourceCurator → FINISH
        """
        # ── MODE 0: Adaptation-only (quiz failure < 50 %) ──────────────────────
        if state.get("is_adaptation_mode", False):
            if not state.get("adaptation_agent_done"):
                next_agent = NodeName.ADAPTATION_ENGINE.value
            else:
                # Adaptation done — format and finish
                next_agent = NodeName.RESPONSE_FORMATTER.value
            return {**state, "next_agent": next_agent}

        is_progression = state.get("is_stage_progression", False)

        if not is_progression:
            # ── MODE 1: Initial build ──────────────────────────────
            if not state.get("profile_agent_done"):
                next_agent = NodeName.PROFILE_ANALYZER.value
            elif not state.get("curriculum_agent_done"):
                next_agent = NodeName.CURRICULUM_GENERATOR.value
            elif not state.get("api_response"):
                # Curriculum ready → format the response before finishing
                next_agent = NodeName.RESPONSE_FORMATTER.value
            else:
                next_agent = "FINISH"
        else:
            # ── MODE 2: Stage progression ──────────────────────────
            if not state.get("curriculum_agent_done"):
                next_agent = NodeName.CURRICULUM_GENERATOR.value
            elif not state.get("stage_extractor_done"):
                next_agent = NodeName.STAGE_EXTRACTOR.value
            elif state.get("learner_progress") and not state.get("adaptation_agent_done"):
                next_agent = NodeName.ADAPTATION_ENGINE.value
            elif not state.get("resource_agent_done"):
                next_agent = NodeName.RESOURCE_CURATOR.value
            elif not state.get("api_response"):
                # Resources ready → format before finishing
                next_agent = NodeName.RESPONSE_FORMATTER.value
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
        """Extracts the single current stage from all_stages by index.

        SHORT-CIRCUIT: If the caller already supplied ``current_stage`` directly
        in the request body, skip all extraction logic and use it as-is.
        This is the preferred (Option A) path — no full curriculum needed.
        """
        self.logger.info("[Node] Running StageExtractor…")

        # ── Option A: current_stage was sent directly in the request ──────────
        if state.get("current_stage"):
            current_stage = state["current_stage"]
            self.logger.info(
                f"[StageExtractor] current_stage provided directly: '{current_stage.get('name')}' — skipping extraction."
            )
            return {
                **state,
                "current_stage": current_stage,
                "stage_extractor_done": True,
                "resource_agent_done": False,
            }

        # ── Option B (legacy): extract from full curriculum by index ──────────
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
            "stage_extractor_done": True,
            "resource_agent_done": False,
        }


    def _response_formatter_node(self, state: RoadmapState) -> Dict[str, Any]:
        """Always last node before END. Builds the frontend-ready JSON response.

        For every mode that produces resources (Mode 0 — adaptation, Mode 2 — stage
        progression), this node first **injects** those resources into the matching
        stage inside the curriculum dict, then returns the **full updated roadmap**
        so the backend can replace its JSONB record with a single write.

        Modes
        -----
        Mode 0 — adaptation (is_adaptation_mode=True)
            Patches the failing stage with ``remedial_resources`` from
            AdaptationEngineOutput (tagged ``resource_type=remedial``).
            Returns full curriculum + adaptation metadata.

        Mode 2 — stage_resources (is_stage_progression=True)
            Patches the current stage with ``stage_resources`` from ResourceCurator.
            Returns full curriculum + stage-level metadata.

        Mode 1 — roadmap_overview (initial build)
            No injection — curriculum returned as-is.
        """
        is_adaptation  = state.get("is_adaptation_mode", False) or bool(state.get("adapted_curriculum"))
        is_progression = state.get("is_stage_progression", False)

        mode = (
            "adaptation"      if is_adaptation  else
            "stage_resources" if is_progression else
            "roadmap_overview"
        )

        # ── Serialiser ────────────────────────────────────────────────────────
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

        # ── Get mutable plain-dict copy of the curriculum ─────────────────────
        curriculum: Dict = to_serializable(state.get("curriculum")) or {}
        stages: List[Dict] = curriculum.get("stages", [])

        # ══════════════════════════════════════════════════════════════════════
        # MODE 0 — ADAPTATION: inject remedial resources into the failing stage
        # ══════════════════════════════════════════════════════════════════════
        if is_adaptation:
            adapted      = to_serializable(state.get("adapted_curriculum")) or {}
            stage_id     = state.get("stage_id")
            remedial     = adapted.get("remedial_resources", [])
            adjustments  = adapted.get("stage_adjustments",  [])
            summary_text = adapted.get("summary",             "")
            score        = adapted.get("score")
            struggling   = adapted.get("struggling_topics",   [])
            failed_qs    = adapted.get("failed_questions",    [])
            next_action  = adapted.get("recommended_next_action", "retry_stage")

            # Tag each remedial resource so the frontend can visually distinguish them
            tagged = [{**r, "resource_type": "remedial"} for r in remedial]

            # Patch the matching stage
            patched = False
            for stage in stages:
                if stage.get("id") == stage_id:
                    stage["resources"]          = stage.get("resources", []) + tagged
                    stage["adapted"]            = True
                    stage["adaptation_summary"] = summary_text
                    patched = True
                    break

            if not patched and stages:
                self.logger.warning(
                    f"[ResponseFormatter] stage_id='{stage_id}' not found — "
                    "injecting into stages[0] as fallback."
                )
                stages[0]["resources"]          = stages[0].get("resources", []) + tagged
                stages[0]["adapted"]            = True
                stages[0]["adaptation_summary"] = summary_text

            curriculum["stages"]             = stages
            curriculum["last_adapted_stage"] = stage_id

            data = {
                # Full updated roadmap — backend replaces its JSONB record with this
                "curriculum":              curriculum,
                # Adaptation metadata
                "stage_id":                stage_id,
                "stage_name":              state.get("stage_name"),
                "score":                   score,
                "struggling_topics":       struggling,
                "failed_questions":        failed_qs,
                "stage_adjustments":       adjustments,
                "summary":                 summary_text,
                "recommended_next_action": next_action,
            }

        # ══════════════════════════════════════════════════════════════════════
        # MODE 2 — STAGE PROGRESSION: inject curated resources into current stage
        # ══════════════════════════════════════════════════════════════════════
        elif is_progression:
            stage_resources  = to_serializable(state.get("stage_resources")) or []
            current_stage    = to_serializable(state.get("current_stage"))   or {}
            current_stage_id = current_stage.get("id")

            patched = False
            for stage in stages:
                if stage.get("id") == current_stage_id:
                    stage["resources"] = stage_resources
                    patched = True
                    break

            if not patched and stages:
                self.logger.warning(
                    f"[ResponseFormatter] current_stage id='{current_stage_id}' not found — "
                    "injecting into stages[0] as fallback."
                )
                stages[0]["resources"] = stage_resources

            curriculum["stages"] = stages

            data = {
                # Full updated roadmap — backend replaces its JSONB record with this
                "curriculum":    curriculum,
                # Stage-level metadata
                "current_stage": current_stage,
                "stage_index":   state.get("current_stage_index"),
                "stage_resources": stage_resources,
            }

        # ══════════════════════════════════════════════════════════════════════
        # MODE 1 — INITIAL ROADMAP BUILD (no resource injection needed)
        # ══════════════════════════════════════════════════════════════════════
        else:
            data = {
                "curriculum": curriculum,
            }

        # ── Pull requested metadata out of curriculum/state into root data ──
        # Backend requested `total_weeks`, `difficulty_level`, and `skill_gaps`
        # at the root level so they can be stored in separate DB columns.
        
        # Determine total_weeks (may be in state or curriculum)
        total_w = state.get("estimated_duration_weeks")
        if total_w is None:
            total_w = curriculum.get("total_weeks")
            
        data["total_weeks"]      = total_w
        data["difficulty_level"] = state.get("difficulty_level")
        data["skill_gaps"]       = state.get("skill_gaps", [])

        # Optionally remove total_weeks from inside the curriculum to keep it DRY
        if "total_weeks" in curriculum:
            del curriculum["total_weeks"]

        return {
            **state,
            "api_response": {
                "status":       "error" if state.get("error") else "success",
                "mode":         mode,
                "user_id":      state.get("user_id"),
                "career_track": state.get("career_track"),
                "data":         data,
                "error":        state.get("error"),
            },
        }

