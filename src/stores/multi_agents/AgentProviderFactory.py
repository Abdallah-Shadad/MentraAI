import logging
from typing import Any, Dict, Optional

from .AgentEnums import AgentType
from .AgentInterface import AgentInterface

# ── Roadmap agents ──────────────────────────────────────────────────────
from .RoadmapMultiAgent.agents.ProfileAnalyzer import ProfileAnalyzer
from .RoadmapMultiAgent.agents.CurriculumGenerator import CurriculumGenerator
from .RoadmapMultiAgent.agents.ResourceCurator import ResourceCurator
from .RoadmapMultiAgent.agents.AdaptationEngine import AdaptationEngine

# ── Quiz agents ──────────────────────────────────────────────────────────
from .QuizAgent.agents.QuestionGenerator import QuestionGenerator

# ── Capstone agents (uncomment when implemented) ────────────────────────
# from .CapstoneMultiAgent.agents.ProjectIdeator import ProjectIdeator
# from .CapstoneMultiAgent.agents.ImplementationGuide import ImplementationGuide
# from .CapstoneMultiAgent.agents.CodeReviewer import CodeReviewer
# from .CapstoneMultiAgent.agents.ProjectEvaluator import ProjectEvaluator
# from .CapstoneMultiAgent.agents.CertificationAgent import CertificationAgent

# ── Chat agents (uncomment when implemented) ────────────────────────────
# from .ChatAgent.agents.ContextRetriever import ContextRetriever
# from .ChatAgent.agents.IntentClassifier import IntentClassifier
# from .ChatAgent.agents.ResponseGenerator import ResponseGenerator
# from .ChatAgent.agents.ConversationManager import ConversationManager


class AgentProviderFactory:
    """
    Factory that instantiates any registered agent by its AgentType enum.

    Design mirrors LLMProviderFactory:
    - __init__  receives a config dict so the factory can pass API keys,
      model IDs, and other settings into each agent on creation.
    - create()  returns a fully configured AgentInterface instance.

    Usage
    -----
    factory = AgentProviderFactory(config=settings)
    agent   = factory.create(AgentType.CURRICULUM_GENERATOR)
    """

    # Registry: maps each AgentType to its concrete class.
    # Add new agents here as you implement them — nothing else changes.
    _REGISTRY: Dict[AgentType, Any] = {
        AgentType.PROFILE_ANALYZER:     ProfileAnalyzer,
        AgentType.CURRICULUM_GENERATOR: CurriculumGenerator,
        AgentType.RESOURCE_CURATOR:     ResourceCurator,
        AgentType.ADAPTATION_ENGINE:    AdaptationEngine,

        # ── Quiz ──────────────────────────────────────────────────────────
        AgentType.QUESTION_GENERATOR:   QuestionGenerator,

        # ── Capstone (add when ready) ──────────────────────────────────
        # AgentType.PROJECT_IDEATOR:      ProjectIdeator,
        # AgentType.IMPLEMENTATION_GUIDE: ImplementationGuide,
        # AgentType.CODE_REVIEWER:        CodeReviewer,
        # AgentType.PROJECT_EVALUATOR:    ProjectEvaluator,
        # AgentType.CERTIFICATION_AGENT:  CertificationAgent,

        # ── Chat (add when ready) ──────────────────────────────────────
        # AgentType.CONTEXT_RETRIEVER:    ContextRetriever,
        # AgentType.INTENT_CLASSIFIER:    IntentClassifier,
        # AgentType.RESPONSE_GENERATOR:   ResponseGenerator,
        # AgentType.CONVERSATION_MANAGER: ConversationManager,
    }

    def __init__(self, config: Any):
        """
        Args:
            config: A settings/config object (or dict) that agents may
                    need for initialisation (LLM API keys, model IDs, etc.).
        """
        self.config = config
        self.logger = logging.getLogger("uvicorn.error")

    # ── Public API ──────────────────────────────────────────────────────

    def create(self, agent_type: AgentType, **kwargs) -> AgentInterface:
        """
        Instantiate and return the agent for *agent_type*.

        Args:
            agent_type: One of the ``AgentType`` enum values.
            **kwargs:   Extra keyword arguments forwarded to the agent's
                        ``__init__`` (e.g. ``llm=...``, ``tools=[...]``).

        Returns:
            A concrete ``AgentInterface`` instance.

        Raises:
            ValueError: If *agent_type* is not registered in the factory.
        """
        agent_class = self._REGISTRY.get(agent_type)

        if agent_class is None:
            self.logger.error(f"[AgentProviderFactory] Unknown agent type: {agent_type}")
            raise ValueError(
                f"Agent type '{agent_type.value}' is not registered in AgentProviderFactory. "
                f"Available types: {[t.value for t in self._REGISTRY]}"
            )

        self.logger.info(f"[AgentProviderFactory] Creating agent: {agent_type.value}")
        return agent_class(config=self.config, **kwargs)

    @staticmethod
    def list_available() -> list:
        """Return the list of all currently registered AgentType values."""
        return [t.value for t in AgentProviderFactory._REGISTRY.keys()]
