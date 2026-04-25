from stores.multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory

config = {
<<<<<<< Updated upstream
    "api_key": "sk-wfonewfofoofe",
    "base_url": "https://8ae4-34-187-223-8.ngrok-free.app/v1/",
    "max_output_tokens": 100000,
    "temperature": 0.1,
    "model": "qwen3:8b",
=======
    "api_key": "AIzaSyD7uC5nJHI5lXE1FCGX5jLOgxmfa8UmRVk",
    "base_url": "https://integrate.api.nvidia.com/v1",
    "max_output_tokens": 100000,
    "temperature": 0.1,
    "model": "qwen/qwen3.5-122b",
>>>>>>> Stashed changes
}

my_llm = OpenAIProvider(
    api_key=config["api_key"],  
    base_url=config["base_url"],
    max_output_tokens=config["max_output_tokens"],
    temperature=config["temperature"],
)
my_llm.set_generation_model(config["model"])


agent_factory = AgentProviderFactory(config)

graph = RoadmapGraph(
    config=config,
    agent_factory=agent_factory,
    llm=my_llm
)

# In LangGraph Studio must Called Graph = app
app= graph.build().compile()
