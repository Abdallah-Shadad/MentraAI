from stores.multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from stores.llm.providers.GeminiProvider import GeminiProvider
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory

from helpers.config import get_llm_config
config = get_llm_config()

my_llm = GeminiProvider(
    api_key=config["api_key"],  
    max_output_tokens=config["max_output_tokens"],
    temperature=config["temperature"],
)
my_llm.set_generation_model("gemini-2.5-flash-lite")


agent_factory = AgentProviderFactory(config)

graph = RoadmapGraph(
    config=config,
    agent_factory=agent_factory,
    llm=my_llm
)

# In LangGraph Studio must Called Graph = app
app= graph.build().compile()
