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

from pydantic import BaseModel
from typing import Optional, Dict, Any

class RoadmapRequest(BaseModel):
    user_id: str
    career_track: str
    weekly_hours: int
    is_stage_progression: Optional[bool] = False
    # --- Stage Progression fields ---
    # Option A (preferred): send the stage directly — no need for full curriculum
    current_stage: Optional[Dict[str, Any]] = None
    # Option B (legacy): send full curriculum + index — StageExtractor will slice it
    curriculum: Optional[Dict[str, Any]] = None
    current_stage_index: Optional[int] = None
    learner_progress: Optional[Dict[str, Any]] = None

@roadmap_router.post("/")
async def roadmap(request_body: RoadmapRequest, app_settings: Settings = Depends(get_settings)):
    try:
        user_id = request_body.user_id
        career_track = request_body.career_track
        weekly_hours = request_body.weekly_hours
        is_stage_progression = request_body.is_stage_progression
        curriculum = request_body.curriculum
        current_stage_index = request_body.current_stage_index
        learner_progress = request_body.learner_progress
        print("RAW BODY:", request_body.model_dump())
        if not user_id or not career_track or not weekly_hours:
            return JSONResponse(
                status_code=status.HTTP_400_BAD_REQUEST,
                content={
                    "message": "user_id, career_track and weekly_hours are required"
                }
            )
        
        from helpers.config import get_llm_config
        config = get_llm_config()

        # The Supervisor uses its OWN dedicated key so it doesn't eat into
        # any individual agent's quota. Falls back to the default key if
        # GEMINI_API_KEY_SUPERVISOR is not set.
        supervisor_key = (
            getattr(app_settings, "GEMINI_API_KEY_SUPERVISOR", "") 
            or app_settings.GEMINI_API_KEY
        )
        my_llm = GeminiProvider(
            api_key=supervisor_key,
            max_output_tokens=8192,   # Supervisor only needs routing decisions, not huge outputs
            temperature=0.1,
        )
        my_llm.set_generation_model("gemini-2.5-flash")

        agent_factory = AgentProviderFactory(config)
        
        current_stage = request_body.current_stage

        # ── Smart flags: skip agents that are not needed ──────────────────────
        # If current_stage is sent directly, there's no need to run
        # CurriculumGenerator or StageExtractor — go straight to ResourceCurator.
        skip_to_resources = bool(is_stage_progression and current_stage)

        # Build initial state and drop None values
        initial_state: RoadmapState = {
            "user_id": user_id,
            "career_track": career_track,
            "weekly_hours": weekly_hours,
            "is_stage_progression": is_stage_progression,
            "current_stage": current_stage,             # Option A: direct stage
            "curriculum": curriculum,                   # Option B: full curriculum (legacy)
            "current_stage_index": current_stage_index,
            "learner_progress": learner_progress,
            # Pre-mark agents as done so Supervisor skips them
            "curriculum_agent_done":  skip_to_resources,
            "stage_extractor_done":   skip_to_resources,
        }

        initial_state = {k: v for k, v in initial_state.items() if v is not None}
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
                "signal": "201_Created",
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