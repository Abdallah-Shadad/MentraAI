"""
routes/track_recommender.py
============================
POST /api/v1/tracks/recommend

Light agent endpoint that analyses a user's profile and recommends
the 3-5 most suitable tech career tracks.

The profile may be partially complete — the agent handles missing fields
gracefully and reports profile_completeness to the caller.

Request body
------------
{
    "user_id": "user_123",
    "profile": {
        "Age": "25-34 years old",                           // optional
        "EdLevel": "Master's degree (M.A., M.S., ...)",     // optional
        "YearsCode": 8,                                      // optional
        "WorkExp": 6,                                        // optional
        "Employment": "Employed",                            // optional
        "RemoteWork": "Remote",                              // optional
        "Industry": "Software Development",                  // optional
        "OrgSize": "100 to 499 employees",                   // optional
        "AISelect": "Yes, I use AI tools daily",             // optional
        "current_skills": ["python", "fastapi", ...],        // optional
        "future_skills": ["rust", "go", ...]                 // optional
    }
}

Response codes
--------------
201 Created              — recommendations generated successfully.
400 Bad Request          — missing user_id or profile entirely.
500 Internal Server Error — graph execution failure.
"""

import logging
import time

from fastapi import APIRouter, Depends, Request, status
from fastapi.responses import JSONResponse
from fastapi.encoders import jsonable_encoder

from helpers.config import get_settings, Settings
from stores.multi_agents.TrackRecommenderAgent.TrackRecommenderGraph import (
    TrackRecommenderGraph,
    TrackRecommenderState,
)
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
from stores.llm.providers.GeminiProvider import GeminiProvider

logger = logging.getLogger("uvicorn.error")

# ── Router ─────────────────────────────────────────────────────────────────────

track_recommender_router = APIRouter(
    prefix="/api/v1/tracks",
    tags=["track-recommender"],
)


# ── Helpers ────────────────────────────────────────────────────────────────────

def _build_llm_and_config(app_settings: Settings):
    """
    Builds the LLM provider from application settings.
    Mirrors the pattern used in routes/quiz.py.
    """
    config = {
        "api_key": app_settings.GEMINI_API_KEY,
        "max_output_tokens": 100000,
        "temperature": 0.1,
        "model": "gemini-2.5-flash",
    }
    llm = GeminiProvider(
        api_key=app_settings.GEMINI_API_KEY,
        max_output_tokens=100_000,
        temperature=0.1,
    )
    llm.set_generation_model("gemini-2.5-flash")
    return llm, config


# ══════════════════════════════════════════════════════════════════════════════
# ENDPOINT — Recommend Tech Tracks
# ══════════════════════════════════════════════════════════════════════════════

@track_recommender_router.post("/recommend", status_code=status.HTTP_201_CREATED)
async def recommend_tracks(
    request: Request,
    app_settings: Settings = Depends(get_settings),
):
    """
    POST /api/v1/tracks/recommend

    Analyses a user's profile (which may be partially complete) and
    returns 3-5 recommended tech career tracks with fit scores,
    reasoning, skill overlap analysis, and transition time estimates.
    """
    start_time = time.perf_counter()

    # ── Parse request body ────────────────────────────────────────────────
    try:
        data = await request.json()
    except Exception as exc:
        logger.error(f"[TrackRecommend] Failed to parse request body: {exc}")
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"signal": "400_Bad_Request", "message": "Invalid JSON body."},
        )

    # ── Validate required fields ──────────────────────────────────────────
    user_id = data.get("user_id")
    profile = data.get("profile")

    if not user_id:
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={
                "signal": "400_Bad_Request",
                "message": "Missing required field: 'user_id'",
            },
        )

    if profile is None or not isinstance(profile, dict):
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={
                "signal": "400_Bad_Request",
                "message": "Missing or invalid 'profile' — must be a JSON object.",
            },
        )

    logger.info(
        f"[TrackRecommend] user={user_id} | "
        f"profile_keys={list(profile.keys())} | "
        f"current_skills_count={len(profile.get('current_skills', []))}"
    )

    # ── Build initial state ───────────────────────────────────────────────
    initial_state: TrackRecommenderState = {
        "user_id": user_id,
        "profile": profile,
    }

    # ── Build LLM + Graph ─────────────────────────────────────────────────
    try:
        llm, config = _build_llm_and_config(app_settings)
        agent_factory = AgentProviderFactory(config)

        graph = TrackRecommenderGraph(
            config=config,
            agent_factory=agent_factory,
            llm=llm,
        )
        app = graph.build().compile()
    except Exception as exc:
        logger.error(f"[TrackRecommend] Graph initialisation failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Track recommender initialisation error: {exc}",
            },
        )

    # ── Invoke graph ──────────────────────────────────────────────────────
    try:
        final_state = app.invoke(initial_state)
    except Exception as exc:
        logger.error(f"[TrackRecommend] Graph execution failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Track recommendation error: {exc}",
            },
        )

    # ── Return response ───────────────────────────────────────────────────
    return JSONResponse(
        status_code=status.HTTP_201_CREATED,
        content=jsonable_encoder({
            "signal":        "201_Created",
            "status":        "success",
            "message":       "Track recommendations generated successfully",
            "recommendations": final_state.get("api_response", {}),
            "time_consumed": time.perf_counter() - start_time,
        }),
    )
