from abc import ABC, abstractmethod
from typing import Any, Dict, Optional

from langgraph.graph import StateGraph
from .GraphEnums import GraphType


class GraphInterface(ABC):
    """
    Abstract base class for every multi-agent supervisor graph.

    All concrete graphs MUST inherit from this class and implement every
    abstract method. The design mirrors AgentInterface / LLMInterface so
    all three factory layers stay consistent.

    Typical lifecycle
    -----------------
    1. Instantiate the concrete graph class (receives config + agent factory).
    2. Call ``build()``     → wires nodes, edges, and conditional routing.
    3. Call ``compile()``   → returns a runnable LangGraph app.
    4. Call ``invoke(state)`` for one-shot execution.
       OR  ``stream(state)``  for streaming step-by-step output.
    """

    # ── Identity ────────────────────────────────────────────────────────

    @abstractmethod
    def get_graph_type(self) -> GraphType:
        """Return the GraphType enum value that identifies this graph."""
        pass

    # ── Graph Construction ──────────────────────────────────────────────

    @abstractmethod
    def build(self) -> "GraphInterface":
        """
        Wire all agent nodes, edges, and conditional routing into the
        internal ``StateGraph``. Must be called before ``compile()``.

        Returns:
            self  (enables fluent chaining: ``graph.build().compile()``)
        """
        pass

    @abstractmethod
    def compile(self) -> Any:
        """
        Compile the ``StateGraph`` into a runnable LangGraph application.

        Returns:
            A compiled LangGraph ``CompiledGraph`` (or ``Pregel``) object.
        """
        pass

    # ── Execution ────────────────────────────────────────────────────────

    @abstractmethod
    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """
        Run the full graph synchronously and return the final state.

        Args:
            state: Initial graph state dict.

        Returns:
            Final state dict after all nodes have executed.
        """
        pass

    @abstractmethod
    def stream(self, state: Dict[str, Any]):
        """
        Run the graph and yield intermediate state updates step-by-step.

        Args:
            state: Initial graph state dict.

        Yields:
            Partial state dicts — one per completed graph step.
        """
        pass

    # ── State Schema ─────────────────────────────────────────────────────

    @abstractmethod
    def get_state_schema(self) -> type:
        """
        Return the TypedDict / Pydantic model class that describes the
        shared state flowing through this graph.
        """
        pass
