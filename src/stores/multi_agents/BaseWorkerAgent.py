"""
multi_agents/BaseWorkerAgent.py
--------------------------------
Template Method base class for every WORKER agent in the system.

Eliminates the boilerplate that was copy-pasted across every agent:
  - __init__          (config / llm / tools / logger)
  - set_llm           
  - bind_tools        
  - set_system_prompt 
  - get_system_prompt 
  - get_agent_role    → always WORKER
  - invoke            → shared try/except skeleton

Subclasses ONLY need to implement:
  1. get_agent_type()       — which AgentType enum value
  2. build_messages(state)  — how to build [System, Human] from state
  3. _output_key()          — e.g. "curriculum", "resources"
  4. _done_key()            — e.g. "curriculum_agent_done"
  5. _output_schema()       — Pydantic / TypedDict class for structured output
"""

import logging
from abc import abstractmethod
from typing import Any, Dict, List, Optional

from langchain_core.tools import BaseTool
from langchain_core.messages import BaseMessage

from .AgentEnums import AgentRole
from .AgentInterface import AgentInterface


class BaseWorkerAgent(AgentInterface):
    """
    Concrete base for all WORKER agents.

    Uses the Template Method Pattern:
    - ``invoke()`` is the *template* — it owns the shared skeleton.
    - Subclasses fill in the blanks via the three small abstract hooks below.
    """

    # Override in every subclass — used as the default system prompt.
    _DEFAULT_SYSTEM_PROMPT: str = ""

    # ── Shared __init__ ─────────────────────────────────────────────────

    def __init__(
        self,
        config: Any = None,
        llm: Any = None,
        tools: Optional[List[BaseTool]] = None,
    ) -> None:
        self.config = config
        self.llm = llm
        self.tools: List[BaseTool] = tools or []
        self._system_prompt: str = self._DEFAULT_SYSTEM_PROMPT
        # Logger name = concrete class name (e.g. "CurriculumGenerator")
        # self.logger = logging.getLogger(self.__class__.__name__)
        self.logger = logging.getLogger("uvicorn.error")

    # ── AgentInterface: role — always WORKER ────────────────────────────

    def get_agent_role(self) -> AgentRole:
        return AgentRole.WORKER

    # ── AgentInterface: LLM / tools — shared implementations ────────────

    def set_llm(self, llm: Any) -> None:
        self.llm = llm
        self.logger.info(f"[{self.__class__.__name__}] LLM set: {type(llm).__name__}")

    def bind_tools(self, tools: List[BaseTool]) -> None:
        self.tools = tools
        if self.llm is not None:
            self.llm.bind_tools(tools)
        self.logger.info(f"[{self.__class__.__name__}] Bound {len(tools)} tool(s).")

    # ── AgentInterface: system prompt — shared implementations ──────────

    def set_system_prompt(self, prompt: str) -> None:
        self._system_prompt = prompt

    def get_system_prompt(self) -> str:
        return self._system_prompt

    # ── Template Method: invoke ──────────────────────────────────────────

    def invoke(self, state: Dict[str, Any]) -> Dict[str, Any]:
        """
        Shared invoke skeleton — subclasses must NOT override this.
        They fill in behaviour through the three hooks below.
        """
        agent_name = self.__class__.__name__

        if self.llm is None:
            raise RuntimeError(
                f"{agent_name} requires an LLM. Call set_llm() first."
            )

        self.logger.info(f"[{agent_name}] Starting execution.")
        messages = self.build_messages(state)

        try:
            # Use self.llm.client directly so we don't mutate self.llm.client
            # (GeminiProvider.with_structured_output replaces self.client in-place,
            # which would break on the second invocation).
            structured_chain = self.llm.client.with_structured_output(self._output_schema())
            response = structured_chain.invoke(messages)
            self.logger.info(f"[{agent_name}] Completed successfully.")
            return {
                **state,
                self._output_key(): response,
                self._done_key(): True,
            }

        except Exception as e:
            self.logger.error(f"[{agent_name}] invoke failed: {e}")
            return {
                **state,
                self._output_key(): None,
                self._done_key(): False,
                "error": str(e),
            }

    # ── Abstract hooks (the only things subclasses must define) ─────────

    @abstractmethod
    def _output_key(self) -> str:
        """
        The state key this agent writes its result to.
        Example: ``"curriculum"``, ``"resources"``, ``"profile_analysis"``
        """
        pass

    @abstractmethod
    def _done_key(self) -> str:
        """
        The boolean flag key that signals completion.
        Example: ``"curriculum_agent_done"``, ``"resource_agent_done"``
        """
        pass

    @abstractmethod
    def _output_schema(self) -> type:
        """
        The Pydantic model or TypedDict passed to ``with_structured_output``.
        Example: ``CurriculumOutput``, ``dict``
        """
        pass