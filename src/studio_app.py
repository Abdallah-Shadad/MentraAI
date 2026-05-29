#from stores.multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph
#from stores.multi_agents.QuizAgent.QuizGraph import QuizGraph
from stores.multi_agents.TrackRecommenderAgent.TrackRecommenderGraph import TrackRecommenderGraph
from stores.multi_agents.ProjectAgent.ProjectGraph import ProjectGraph
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from stores.llm.providers.GeminiProvider import GeminiProvider
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
import os
from dotenv import load_dotenv
load_dotenv()


from helpers.config import get_llm_config
config = get_llm_config()

my_llm = GeminiProvider(
    api_key=config["api_key"],  
    max_output_tokens=config["max_output_tokens"],
    temperature=config["temperature"],
)
my_llm.set_generation_model("gemini-2.5-flash-lite")


agent_factory = AgentProviderFactory(config)

graph = TrackRecommenderGraph(
    config=config,
    agent_factory=agent_factory,
    llm=my_llm
)

# In LangGraph Studio must Called Graph = app
app = graph.build().compile()
