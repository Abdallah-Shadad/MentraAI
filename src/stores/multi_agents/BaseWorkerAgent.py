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
from langchain_core.messages import BaseMessage, ToolMessage

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

        Tool-calling flow (when self.tools is non-empty):
          1. Bind tools to the LLM and call it.
          2. Execute any tool calls the LLM made, append ToolMessages.
          3. Repeat until the LLM stops calling tools.
          4. Pass the full message history to with_structured_output.

        If no tools are registered, steps 1-3 are skipped and only
        the structured output call (step 4) runs — identical to before.
        """
        agent_name = self.__class__.__name__

        if self.llm is None:
            raise RuntimeError(
                f"{agent_name} requires an LLM. Call set_llm() first."
            )

        self.logger.info(f"[{agent_name}] Starting execution.")
        messages = self.build_messages(state)

        try:
            # ── Tool-calling loop (skipped when no tools are registered) ──
            if self.tools:
                llm_with_tools = self.llm.client.bind_tools(self.tools)
                self.logger.info(f"[{agent_name}] Running with {len(self.tools)} tool(s).")

                while True:
                    ai_msg = llm_with_tools.invoke(messages)
                    messages.append(ai_msg)

                    if not ai_msg.tool_calls:
                        break  # LLM finished calling tools → move to structured output

                    for tc in ai_msg.tool_calls:
                        tool_fn = next((t for t in self.tools if t.name == tc["name"]), None)
                        if tool_fn is None:
                            self.logger.warning(f"[{agent_name}] Unknown tool requested: {tc['name']}")
                            continue
                        result = tool_fn.invoke(tc["args"])
                        self.logger.info(f"[{agent_name}] Tool '{tc['name']}' executed successfully.")
                        messages.append(ToolMessage(content=str(result), tool_call_id=tc["id"]))

            # ── Final structured output (uses full message history w/ tool context) ──
            # Use self.llm.client directly so we don't mutate self.llm in-place.
            # (GeminiProvider.with_structured_output replaces self.client in-place,
            # which would break on the second invocation.)
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