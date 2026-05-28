from enum import Enum


class AgentType(Enum):
    """All agent types across all Multi-Agent Supervisors."""

    # ── Roadmap Supervisor ──────────────────────────────────────────────
    PROFILE_ANALYZER       = "profile_analyzer"
    CURRICULUM_GENERATOR   = "curriculum_generator"
    RESOURCE_CURATOR       = "resource_curator"
    ADAPTATION_ENGINE      = "adaptation_engine"

    # ── Quiz Supervisor ─────────────────────────────────────────────────
    QUESTION_GENERATOR     = "question_generator"
    ANSWER_EVALUATOR       = "answer_evaluator"
    HINT_PROVIDER          = "hint_provider"
    QUIZ_GRADER            = "quiz_grader"
    FEEDBACK_GENERATOR     = "feedback_generator"
    REMEDIATION_PLANNER    = "remediation_planner"

    # ── Project Supervisor ──────────────────────────────────────────────
    PROJECT_RECOMMENDER    = "project_recommender"

    # ── Capstone Supervisor ─────────────────────────────────────────────
    PROJECT_IDEATOR        = "project_ideator"
    IMPLEMENTATION_GUIDE   = "implementation_guide"
    CODE_REVIEWER          = "code_reviewer"
    PROJECT_EVALUATOR      = "project_evaluator"
    CERTIFICATION_AGENT    = "certification_agent"

    # ── Chat Supervisor ─────────────────────────────────────────────────
    CONTEXT_RETRIEVER      = "context_retriever"
    INTENT_CLASSIFIER      = "intent_classifier"
    RESPONSE_GENERATOR     = "response_generator"
    CONVERSATION_MANAGER   = "conversation_manager"


class AgentStatus(Enum):
    """Runtime status of an agent node."""
    IDLE       = "idle"
    RUNNING    = "running"
    COMPLETED  = "completed"
    FAILED     = "failed"
    SKIPPED    = "skipped"


class AgentRole(Enum):
    """High-level role of an agent within a graph."""
    SUPERVISOR = "supervisor"
    WORKER     = "worker"
