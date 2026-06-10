import logging
from typing import Any, Dict

from .GraphEnums import GraphType
from .GraphInterface import GraphInterface

# ── Supervisor graphs (import as implemented) ────────────────────────────
from ..multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph

# from ..multi_agents.QuizMultiAgent.QuizGraph import QuizGraph
# from ..multi_agents.CapstoneMultiAgent.CapstoneGraph import CapstoneGraph
# from ..multi_agents.ChatAgent.ChatGraph import ChatGraph


class GraphProviderFactory:
    """
    Factory that instantiates any registered multi-agent supervisor graph
    by its ``GraphType`` enum value.

    Design mirrors ``AgentProviderFactory`` and ``LLMProviderFactory``:
    - ``__init__`` receives a config object and an agent factory so every
      graph can build its own agents without reaching outside the factory.
    - ``create()`` returns a fully initialised ``GraphInterface`` instance
      ready to be built and compiled.

    Usage
    -----
    ::

        llm_factory   = LLMProviderFactory(config)
        agent_factory = AgentProviderFactory(config)
        graph_factory = GraphProviderFactory(config, agent_factory)

        graph = graph_factory.create(GraphType.ROADMAP_GRAPH)
        app   = graph.build().compile()
        result = app.invoke(initial_state)
    """

    # Registry: maps each GraphType to its concrete class.
    # Add new graphs here as you implement them — nothing else changes.
    _REGISTRY: Dict[GraphType, Any] = {
        GraphType.ROADMAP_GRAPH:  RoadmapGraph,
        # GraphType.QUIZ_GRAPH:     QuizGraph,
        # GraphType.CAPSTONE_GRAPH: CapstoneGraph,
        # GraphType.CHAT_GRAPH:     ChatGraph,
    }

    def __init__(self, config: Any, agent_factory: Any = None):
        """
        Args:
            config:        Project-wide settings / config object.
            agent_factory: An ``AgentProviderFactory`` instance. Passed
                           into each graph so graphs can create their own
                           agents without tight coupling.
        """
        self.config = config
        self.agent_factory = agent_factory
        self.logger = logging.getLogger("uvicorn.error")

    # ── Public API ──────────────────────────────────────────────────────

    def create(self, graph_type: GraphType, **kwargs) -> GraphInterface:
        """
        Instantiate and return the graph for *graph_type*.

        Args:
            graph_type: One of the ``GraphType`` enum values.
            **kwargs:   Extra keyword arguments forwarded to the graph's
                        ``__init__`` (e.g. ``llm=...``).

        Returns:
            A concrete ``GraphInterface`` instance (not yet compiled).

        Raises:
            ValueError: If *graph_type* is not registered.
        """
        graph_class = self._REGISTRY.get(graph_type)

        if graph_class is None:
            self.logger.error(f"[GraphProviderFactory] Unknown graph type: {graph_type}")
            raise ValueError(
                f"Graph type '{graph_type.value}' is not registered. "
                f"Available: {[g.value for g in self._REGISTRY]}"
            )

        self.logger.info(f"[GraphProviderFactory] Creating graph: {graph_type.value}")
        return graph_class(
            config=self.config,
            agent_factory=self.agent_factory,
            **kwargs,
        )

    @staticmethod
    def list_available() -> list:
        """Return the list of all currently registered GraphType values."""
        return [g.value for g in GraphProviderFactory._REGISTRY.keys()]
