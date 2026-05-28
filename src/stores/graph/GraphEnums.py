from enum import Enum


class GraphType(Enum):
    """All multi-agent supervisor graphs in the MentraAI system."""
    ROADMAP_GRAPH  = "roadmap_graph"
    QUIZ_GRAPH     = "quiz_graph"
    PROJECT_GRAPH  = "project_graph"
    CAPSTONE_GRAPH = "capstone_graph"
    CHAT_GRAPH     = "chat_graph"


class GraphStatus(Enum):
    """Runtime status of a graph execution."""
    IDLE       = "idle"
    RUNNING    = "running"
    COMPLETED  = "completed"
    FAILED     = "failed"


class NodeName(Enum):
    """
    Canonical node name strings used as LangGraph node identifiers.
    Using an enum guarantees that add_node() and add_edge() calls
    never drift from each other due to a typo.
    """
    # ── Shared ──────────────────────────────────────────────────────────
    START      = "__start__"
    END        = "__end__"
    SUPERVISOR = "supervisor"

    # ── Roadmap Graph ────────────────────────────────────────────────────
    PROFILE_ANALYZER     = "profile_analyzer"
    CURRICULUM_GENERATOR = "curriculum_generator"
    STAGE_EXTRACTOR      = "stage_extractor"
    RESOURCE_CURATOR     = "resource_curator"
    ADAPTATION_ENGINE    = "adaptation_engine"
    RESPONSE_FORMATTER   = "response_formatter"

    # ── Quiz Graph ───────────────────────────────────────────────────────
    QUESTION_GENERATOR   = "question_generator"
    ANSWER_EVALUATOR     = "answer_evaluator"
    HINT_PROVIDER        = "hint_provider"
    QUIZ_GRADER          = "quiz_grader"
    FEEDBACK_GENERATOR   = "feedback_generator"
    REMEDIATION_PLANNER  = "remediation_planner"

    # ── Project Graph ────────────────────────────────────────────────────
    PROJECT_RECOMMENDER  = "project_recommender"

    # ── Capstone Graph ───────────────────────────────────────────────────
    PROJECT_IDEATOR      = "project_ideator"
    IMPLEMENTATION_GUIDE = "implementation_guide"
    CODE_REVIEWER        = "code_reviewer"
    PROJECT_EVALUATOR    = "project_evaluator"
    CERTIFICATION_AGENT  = "certification_agent"

    # ── Chat Graph ───────────────────────────────────────────────────────
    # Maybe Use it As Agentic Chat 
    CONTEXT_RETRIEVER    = "context_retriever"
    INTENT_CLASSIFIER    = "intent_classifier"
    RESPONSE_GENERATOR   = "response_generator"
    CONVERSATION_MANAGER = "conversation_manager"
