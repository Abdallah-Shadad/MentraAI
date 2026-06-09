from abc import ABC, abstractmethod
from typing import Any, Dict, List, Optional

from langchain_core.tools import BaseTool
from langchain_core.messages import BaseMessage

from .AgentEnums import AgentType, AgentRole


class AgentInterface(ABC):
    """
    Abstract base class for every agent in the MentraAI multi-agent system.

    All concrete agents MUST inherit from this class and implement every
    abstract method. The design mirrors LLMInterface so that the two
    factories (LLMProviderFactory and AgentProviderFactory) stay consistent.
    """

    # ── Identity ────────────────────────────────────────────────────────

    @abstractmethod
    def get_agent_type(self) -> AgentType:
        """Return the enum value that identifies this agent."""
        pass

    @abstractmethod
    def get_agent_role(self) -> AgentRole:
        """Return whether this agent is a SUPERVISOR or a WORKER."""
        pass

    # ── LLM / Tool Binding ──────────────────────────────────────────────

    @abstractmethod
    def set_llm(self, llm: Any) -> None:
        """
        Bind an LLMInterface-compatible provider (OpenAI, Gemini, …)  
        to this agent so it can generate responses.
        """
        pass

    @abstractmethod
    def bind_tools(self, tools: List[BaseTool]) -> None:
        """
        Attach LangChain tools (callables / tool-nodes) that this agent
        may invoke during its execution.
        """
        pass

    # ── System Prompt ───────────────────────────────────────────────────
    @abstractmethod
    def set_system_prompt(self, prompt: str) -> None:
        """
        Set the system-level instruction that governs how this agent
        reasons and responds.
        """
        pass

    @abstractmethod
    def get_system_prompt(self) -> str:
        """Return the current system prompt as a string."""
        pass

    # ── Invocation ──────────────────────────────────────────────────────
    @abstractmethod
    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """
        Execute the agent's core logic given the current graph state.

        Args:
            state: LangGraph state dict (shared across all agent nodes).

        Returns:
            Updated state dict with this agent's output merged in.
        """
        pass

    # ── Message helpers ─────────────────────────────────────────────────
    @abstractmethod
    def build_messages(self, state: Dict[str, Any]) -> List[BaseMessage]:
        """
        Convert relevant fields from `state` into a list of
        LangChain BaseMessage objects ready to be sent to the LLM.
        """
        pass
