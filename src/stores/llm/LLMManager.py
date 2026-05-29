import logging
from typing import Any, Dict, List, Optional

from langchain_core.runnables import Runnable, RunnableLambda
from .providers.GeminiProvider import GeminiProvider
from .providers.OpenAIProvider import OpenAIProvider
from .providers.GroqProvider import GroqProvider

class LLMManager:
    """
    Manages the creation and configuration of LLMs, including setting up
    fallback models (e.g., falling back from Gemini to Groq on rate limits).
    """

    def __init__(self, config: Dict[str, Any]):
        self.config = config
        self.logger = logging.getLogger("uvicorn.error")

    def create_provider(self, provider_config: Dict[str, Any]):
        """
        Creates a raw LLMInterface provider based on a config dict.
        Example provider_config:
        {
            "provider": "gemini",
            "model": "gemini-2.5-flash",
            "api_key": "YOUR_KEY",
            "temperature": 0.1
        }
        """
        provider_name = provider_config.get("provider", "gemini").lower()
        api_key = provider_config.get("api_key")
        model = provider_config.get("model")
        temperature = provider_config.get("temperature", 0.1)
        max_tokens = provider_config.get("max_output_tokens", 8000)

        if not api_key:
            self.logger.warning(f"No API key found for provider '{provider_name}'.")

        if provider_name == "gemini":
            provider = GeminiProvider(api_key=api_key, max_output_tokens=max_tokens, temperature=temperature)
            if model: provider.set_generation_model(model)
        elif provider_name == "openai":
            base_url = provider_config.get("base_url")
            provider = OpenAIProvider(api_key=api_key, base_url=base_url, max_output_tokens=max_tokens, temperature=temperature)
            if model: provider.set_generation_model(model)
        elif provider_name == "groq":
            provider = GroqProvider(api_key=api_key, max_output_tokens=max_tokens, temperature=temperature)
            if model: provider.set_generation_model(model)
        else:
            raise ValueError(f"Unknown provider: {provider_name}")

        return provider

    def get_structured_chain(self, agent_name: str, schema: Any) -> Runnable:
        """
        Creates a Runnable chain with structured output, automatically wrapping it
        in fallbacks if configured.
        """
        # 1. Fetch agent-specific config, or default to a global config
        agent_configs = self.config.get("agent_llm_configs", {})
        config_for_agent = agent_configs.get(agent_name, self.config.get("default_llm_config", {}))

        # Default to a generic Gemini config if nothing is defined
        if not config_for_agent:
            config_for_agent = {
                "primary": {
                    "provider": "gemini", 
                    "model": "gemini-2.5-flash", 
                    "api_key": self.config.get("api_key", "")
                }
            }

        # 2. Build Primary Chain
        primary_config = config_for_agent.get("primary", {})
        primary_provider = self.create_provider(primary_config)
        
        if not primary_provider.client:
            raise RuntimeError(f"Primary provider client not initialized for {agent_name}")
            
        primary_chain = primary_provider.client.with_structured_output(schema)

        # 3. Build Fallback Chains
        fallback_configs = config_for_agent.get("fallbacks", [])
        if not isinstance(fallback_configs, list):
            fallback_configs = [fallback_configs] if fallback_configs else []

        fallback_chains = []
        for f_conf in fallback_configs:
            f_provider = self.create_provider(f_conf)
            if f_provider.client:
                fallback_chains.append(f_provider.client.with_structured_output(schema))

        # 4. Apply fallbacks if available
        if fallback_chains:
            primary_label  = f"{primary_config.get('provider','?')}:{primary_config.get('model','?')}"
            fallback_labels = [f"{f.get('provider','?')}:{f.get('model','?')}" for f in fallback_configs]

            def _log_primary(inputs, label=primary_label, name=agent_name):
                logging.getLogger("uvicorn.error").info(f"[{name}] Using PRIMARY model → {label}")
                return inputs
            def _log_fallback(inputs, labels=fallback_labels, name=agent_name):
                logging.getLogger("uvicorn.error").info(f"[{name}] PRIMARY failed → switching to FALLBACK {labels}")
                return inputs

            logged_primary   = RunnableLambda(_log_primary)  | primary_chain
            logged_fallbacks = [RunnableLambda(_log_fallback) | fc for fc in fallback_chains]

            self.logger.info(f"[{agent_name}] Attaching {len(fallback_chains)} fallback model(s). Primary={primary_label}")
            return logged_primary.with_fallbacks(
                logged_fallbacks,
                exceptions_to_handle=(Exception,),
            )
        
        return primary_chain

    def get_tool_chain(self, agent_name: str, tools: list) -> Runnable:
        """
        Creates a Runnable chain bound to tools, automatically wrapping it
        in fallbacks if configured.
        """
        agent_configs = self.config.get("agent_llm_configs", {})
        config_for_agent = agent_configs.get(agent_name, self.config.get("default_llm_config", {}))

        if not config_for_agent:
            config_for_agent = {
                "primary": {
                    "provider": "gemini", 
                    "model": "gemini-2.5-flash", 
                    "api_key": self.config.get("api_key", "")
                }
            }

        primary_config = config_for_agent.get("primary", {})
        primary_provider = self.create_provider(primary_config)
        
        if not primary_provider.client:
            raise RuntimeError(f"Primary provider client not initialized for {agent_name}")
            
        primary_chain = primary_provider.client.bind_tools(tools)

        fallback_configs = config_for_agent.get("fallbacks", [])
        if not isinstance(fallback_configs, list):
            fallback_configs = [fallback_configs] if fallback_configs else []

        fallback_chains = []
        for f_conf in fallback_configs:
            f_provider = self.create_provider(f_conf)
            if f_provider.client:
                fallback_chains.append(f_provider.client.bind_tools(tools))

        if fallback_chains:
            primary_label   = f"{primary_config.get('provider','?')}:{primary_config.get('model','?')}"
            fallback_labels = [f"{f.get('provider','?')}:{f.get('model','?')}" for f in fallback_configs]

            def _log_primary_t(inputs, label=primary_label, name=agent_name):
                logging.getLogger("uvicorn.error").info(f"[{name}] Using PRIMARY model → {label}")
                return inputs
            def _log_fallback_t(inputs, labels=fallback_labels, name=agent_name):
                logging.getLogger("uvicorn.error").info(f"[{name}] PRIMARY failed → switching to FALLBACK {labels}")
                return inputs

            logged_primary   = RunnableLambda(_log_primary_t)  | primary_chain
            logged_fallbacks = [RunnableLambda(_log_fallback_t) | fc for fc in fallback_chains]

            self.logger.info(f"[{agent_name}] Attaching {len(fallback_chains)} tool fallback model(s). Primary={primary_label}")
            return logged_primary.with_fallbacks(
                logged_fallbacks,
                exceptions_to_handle=(Exception,),
            )
        
        return primary_chain
