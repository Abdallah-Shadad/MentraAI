"""
routes/AdaptionEngine.py
========================
POST /api/v1/quiz/adaptation_stage

Triggered when a learner scores **below 50 %** on a stage quiz.
The endpoint feeds the quiz data into the RoadmapMultiAgent graph with
``is_adaptation_mode = True``, which causes the Supervisor to route
directly to the AdaptationEngine → ResponseFormatter pipeline.

Returns the **full updated roadmap** ready to be stored as a JSONB record
in the database.

Response codes
--------------
201 Created   — adaptation completed; body contains the adapted roadmap.
400 Bad Request — missing required fields or score ≥ 50 %.
500 Internal Server Error — graph execution failure.
"""

import logging
from fastapi import APIRouter, Depends, Request, status
from fastapi.responses import JSONResponse
from fastapi.encoders import jsonable_encoder
from pydantic import BaseModel
from typing import List, Optional

from helpers.config import get_settings, Settings
from stores.multi_agents.RoadmapMultiAgent.RoadmapGraph import RoadmapGraph, RoadmapState
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
from stores.llm.providers.OpenAIProvider import OpenAIProvider
from stores.llm.providers.GeminiProvider import GeminiProvider
import time

logger = logging.getLogger("uvicorn.error")
settings = get_settings()
# ── Router ─────────────────────────────────────────────────────────────────────
adaptation_router = APIRouter(
    prefix="/api/v1/quiz",
    tags=["quiz-adaptation"],
)

# ── Schemas ────────────────────────────────────────────────────────────────────

class FailedQuestion(BaseModel):
    question: str
    user_answer: str
    correct_answer: str

class AdaptationRequest(BaseModel):
    user_id: str
    career_track: str
    stage_id: str
    stage_name: str
    score: float
    failed_questions: List[FailedQuestion]
    learning_objectives: Optional[List[str]] = []
    difficulty_level: Optional[str] = "beginner"

# ── Helpers ────────────────────────────────────────────────────────────────────

def _build_llm(app_settings: Settings):
    """
    Builds the LLM provider from application settings.
    Mirrors the pattern used in routes/roadmap.py so configuration
    stays consistent across both endpoints.
    """
    from helpers.config import get_llm_config
    config = get_llm_config()

    # The Supervisor uses its OWN dedicated key or defaults to standard
    supervisor_key = (
        getattr(app_settings, "GEMINI_API_KEY_SUPERVISOR", "") 
        or app_settings.GEMINI_API_KEY
    )
    llm = GeminiProvider(
        api_key=supervisor_key,
        max_output_tokens=8192,
        temperature=0.1,
    )
    llm.set_generation_model("gemini-2.5-flash-lite")  # 1500 req/day — enough for routing decisions
    
    return llm, config


def _validate_body(data: dict) -> tuple[bool, str]:
    """
    Returns (is_valid, error_message).
    Validates all required fields and enforces the < 70 % score rule.
    """
    required = ["user_id", "career_track", "stage_id", "stage_name", "score", "failed_questions"]
    for field in required:
        if field not in data:
            return False, f"Missing required field: '{field}'"

    score = data.get("score")
    if not isinstance(score, (int, float)):
        return False, "score must be a number"
    if score >= 70:
        return False, (
            f"Adaptation not triggered — learner scored {score}% which is ≥ 70%. "
            "Adaptation only runs when score is below 70%."
        )
    return True, ""


# ── Endpoint ───────────────────────────────────────────────────────────────────

@adaptation_router.post("/adaptation_stage", status_code=status.HTTP_201_CREATED)
async def adaptation_stage(
    request_data: AdaptationRequest,
    app_settings: Settings = Depends(get_settings),
):
    """
    POST /api/v1/quiz/adaptation_stage

    Triggers roadmap-stage adaptation when a learner scores below 50 % on a quiz.
    Invokes the RoadmapMultiAgent graph in **adaptation-only mode** so that the
    Supervisor skips profile / curriculum generation and routes directly to the
    AdaptationEngine, which searches for remedial resources and patches the stage.

    Returns the complete updated roadmap JSON (to be stored as JSONB).
    """
    start_time = time.perf_counter()
    # ── 1. Parse body ──────────────────────────────────────────────────────────
    try:
        # We still support raw JSON in case it's needed, but Pydantic handles the main validation
        data = request_data.model_dump() if hasattr(request_data, 'model_dump') else request_data.dict()
    except Exception as exc:
        logger.error(f"[AdaptationStage] Failed to parse request body: {exc}")
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"signal": "400_Bad_Request", "message": "Invalid JSON body."},
        )

    # ── 2. Validate ────────────────────────────────────────────────────────────
    is_valid, error_msg = _validate_body(data)
    if not is_valid:
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"signal": "400_Bad_Request", "message": error_msg},
        )

    # ── 3. Extract fields ──────────────────────────────────────────────────────
    user_id          = data["user_id"]
    career_track     = data["career_track"]
    stage_id         = data["stage_id"]
    stage_name       = data["stage_name"]
    difficulty_level = data.get("difficulty_level", "beginner")
    score            = data["score"]
    failed_questions = data["failed_questions"]
    learning_objectives = data.get("learning_objectives", [])

    logger.info(
        f"[AdaptationStage] user={user_id} | career={career_track} "
        f"| stage={stage_id} | stage_name={stage_name} | score={score}%"
    )

    # ── 4. Build learner_progress payload consumed by AdaptationEngine ─────────
    # This becomes the primary context the AdaptationEngine agent reads from state.
    learner_progress = {
        "stage_id":          stage_id,
        "stage_name":        stage_name,
        "difficulty_level":  difficulty_level,
        "score":             score,
        "failed":            True,          # guaranteed by validation above (< 50%)
        "failed_questions":  failed_questions,
    }

    # ── 5. Build initial graph state ───────────────────────────────────────────
    initial_state: RoadmapState = {
        "user_id":             user_id,
        "career_track":        career_track,
        "difficulty_level":    difficulty_level,
        "stage_id":            stage_id,           # NEW state key (added to RoadmapState)
        "stage_name":          stage_name,         # NEW state key
        "learner_progress":    learner_progress,
        "is_adaptation_mode":  True,               # NEW flag — skips full pipeline
        "is_stage_progression": False,             # not a stage-progression call
        # Provide minimal curriculum context so AdaptationEngine has something to patch
        "curriculum": {
            "stages": [
                {
                    "id":     stage_id,
                    "name":   stage_name,
                    "topics": [],
                    "learning_objectives": learning_objectives,
                    "estimated_weeks": 1,
                }
            ]
        },
    }

    # ── 6. Initialise graph ────────────────────────────────────────────────────
    try:
        llm, config = _build_llm(app_settings)
        agent_factory = AgentProviderFactory(config)

        graph = RoadmapGraph(
            config=config,
            agent_factory=agent_factory,
            llm=llm,
        )
        app_graph = graph.build().compile()

    except Exception as exc:
        logger.error(f"[AdaptationStage] Graph initialisation failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal":  "500_Internal_Server_Error",
                "status":  "error",
                "message": f"Graph initialisation error: {exc}",
            },
        )

    # ── 7. Invoke graph ────────────────────────────────────────────────────────
    try:
        final_state = app_graph.invoke(initial_state)
    except Exception as exc:
        logger.error(f"[AdaptationStage] Graph execution failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal":  "500_Internal_Server_Error",
                "status":  "error",
                "message": f"Graph execution error: {exc}",
            },
        )

    # ── 8. Shape response ──────────────────────────────────────────────────────
    api_response = final_state.get("api_response", {})

    roadmap_payload = {
        "status":       "FINISH",
        "mode":         "stage_resources",
        "user_id":      user_id,
        "career_track": career_track,
        "stage_id":     stage_id,
        "stage_name":   stage_name,
        "score":        score,
        "adapted":      True,
        **api_response,   # merges data, error, etc. from ResponseFormatter
    }

    return JSONResponse(
        status_code=status.HTTP_201_CREATED,
        content=jsonable_encoder({
            "signal":  "201_Created",
            "status":  "success",
            "message": "Roadmap adaptation successfully",
            "Additional_Resource": roadmap_payload,
            "time_consume": time.perf_counter() - start_time,
        }),
    )
