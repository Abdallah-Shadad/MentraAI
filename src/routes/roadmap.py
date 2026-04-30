from fastapi import FastAPI,APIRouter, Depends,UploadFile,status, Request
from fastapi.responses import JSONResponse
from fastapi.encoders import jsonable_encoder
import os
from helpers.config import get_settings,Settings
import logging
from stores.multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph, RoadmapState
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from stores.llm.providers.GeminiProvider import GeminiProvider

logger = logging.getLogger("uvicorn.error")

roadmap_router = APIRouter(
    prefix="/api/v1/roadmap",
    tags=["roadmap"]
)

@roadmap_router.post("/")
async def roadmap(request: Request,app_settings: Settings = Depends(get_settings)):
    try:
        data = await request.json()
        user_id = data.get("user_id")
        career_track = data.get("career_track")
        weekly_hours = data.get("weekly_hours")
        is_stage_progression = data.get("is_stage_progression")
        print("RAW BODY:", data)
        if not user_id or not career_track or not weekly_hours:
            return JSONResponse(
                status_code=status.HTTP_400_BAD_REQUEST,
                content={
                    "message": "user_id, career_track and weekly_hours are required"
                }
            )
        
        config = {
            "api_key": "sk-wfonewfofoofe",
            "base_url": "https://8ae4-34-187-223-8.ngrok-free.app/v1/",
            "max_output_tokens": 100000,
            "temperature": 0.1,
            "model": "qwen3:8b",
        }

        # my_llm = OpenAIProvider(
        #     api_key=config["api_key"],  
        #     base_url=config["base_url"],
        #     max_output_tokens=config["max_output_tokens"],
        #     temperature=config["temperature"],
        # )
        my_llm = GeminiProvider(
            api_key=app_settings.GEMINI_API_KEY,
            max_output_tokens=100000,
            temperature=0.1,
        )
        my_llm.set_generation_model("gemini-2.5-flash")


        agent_factory = AgentProviderFactory(config)
        initial_state: RoadmapState = {
            "user_id": user_id,
            "career_track": career_track,
            "weekly_hours": weekly_hours,
            "is_stage_progression": is_stage_progression
        }
        graph = RoadmapGraph(
            config=config,
            agent_factory=agent_factory,
            llm=my_llm
        )

        app = graph.build().compile()
        final_state = app.invoke(initial_state)
        return JSONResponse(
            status_code=status.HTTP_201_CREATED,
            content=jsonable_encoder({
                "message": "Roadmap generated successfully",
                "roadmap": final_state.get("api_response", final_state)
            })
        )
    except Exception as e:
        logger.error(f"Error generating roadmap: {e}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "error": f"Error generating roadmap: {e}"
            }
        )