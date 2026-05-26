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
import json

from pydantic import ValidationError

from langchain_core.tools import BaseTool
from langchain_core.messages import BaseMessage, ToolMessage, SystemMessage

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
        llm_manager: Any = None,
    ) -> None:
        self.config = config
        self.llm = llm
        self.llm_manager = llm_manager
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

        if self.llm is None and self.llm_manager is None:
            raise RuntimeError(
                f"{agent_name} requires an LLM or LLMManager. Call set_llm() first."
            )

        self.logger.info(f"[{agent_name}] Starting execution.")
        messages = self.build_messages(state)

        try:
            # ── Tool-calling loop (skipped when no tools are registered) ──
            if self.tools:
                schema_name = self._output_schema().__name__
                
                # Bind regular tools + the output schema as a tool
                if self.llm_manager is not None:
                    llm_with_tools = self.llm_manager.get_tool_chain(agent_name, self.tools + [self._output_schema()])
                else:
                    llm_with_tools = self.llm.client.bind_tools(self.tools + [self._output_schema()])
                    
                self.logger.info(f"[{agent_name}] Running with {len(self.tools)} tool(s) + {schema_name} schema.")

                # Instruct the model to use the schema tool for its final answer
                messages.append(SystemMessage(
                    content=f"When you have finished using tools and are ready to provide the final structured output, "
                            f"you MUST call the `{schema_name}` tool with your final data. "
                            f"DO NOT return the final JSON as plain text."
                ))

                while True:
                    ai_msg = llm_with_tools.invoke(messages)
                    messages.append(ai_msg)

                    if not ai_msg.tool_calls:
                        try:
                            content = ai_msg.content.strip()
                            if content.startswith("```json"):
                                content = content[7:-3].strip()
                            elif content.startswith("```"):
                                content = content[3:-3].strip()
                            
                            parsed_data = json.loads(content)
                            response = self._output_schema()(**parsed_data)
                            self.logger.info(f"[{agent_name}] Parsed structured output directly from plain text.")
                            
                            output_dict = response.model_dump() if hasattr(response, "model_dump") else response.dict() if hasattr(response, "dict") else {}
                            
                            return {
                                **state,
                                **output_dict,
                                self._output_key(): response,
                                self._done_key(): True,
                            }
                        except Exception as e:
                            self.logger.info(f"[{agent_name}] No tool calls made and direct parse failed. Breaking to structured output fallback.")
                            break

                    # Check if the final schema tool was called
                    schema_call = next((tc for tc in ai_msg.tool_calls if tc["name"] == schema_name), None)
                    if schema_call:
                        self.logger.info(f"[{agent_name}] Final output schema tool called.")
                        try:
                            response = self._output_schema()(**schema_call["args"])
                            
                            output_dict = response.model_dump() if hasattr(response, "model_dump") else response.dict() if hasattr(response, "dict") else {}
                            
                            return {
                                **state,
                                **output_dict,
                                self._output_key(): response,
                                self._done_key(): True,
                            }
                        except Exception as e:
                            self.logger.warning(f"[{agent_name}] Failed to parse schema tool args: {e}. Falling back.")
                            break

                    for tc in ai_msg.tool_calls:
                        if tc["name"] == schema_name:
                            continue
                        tool_fn = next((t for t in self.tools if t.name == tc["name"]), None)
                        if tool_fn is None:
                            self.logger.warning(f"[{agent_name}] Unknown tool requested: {tc['name']}")
                            continue
                        result = tool_fn.invoke(tc["args"])
                        self.logger.info(f"[{agent_name}] Tool '{tc['name']}' executed successfully.")
                        messages.append(ToolMessage(content=str(result), tool_call_id=tc["id"]))

            # ── Final structured output (Fallback or No-Tools Path) ──
            if self.llm_manager is not None:
                structured_chain = self.llm_manager.get_structured_chain(agent_name, self._output_schema())
            else:
                structured_chain = self.llm.client.with_structured_output(self._output_schema())
                
            response = structured_chain.invoke(messages)
            self.logger.info(f"[{agent_name}] Completed successfully.")
            
            output_dict = response.model_dump() if hasattr(response, "model_dump") else response.dict() if hasattr(response, "dict") else {}
            
            return {
                **state,
                **output_dict,
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