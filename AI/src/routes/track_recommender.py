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

from pydantic import BaseModel
from typing import Dict, Any

class TrackRecommendRequest(BaseModel):
    user_id: str
    profile: Dict[str, Any]


@track_recommender_router.post("/recommend", status_code=status.HTTP_201_CREATED)
async def recommend_tracks(
    request_body: TrackRecommendRequest,
    app_settings: Settings = Depends(get_settings),
):
    """
    POST /api/v1/tracks/recommend

    Analyses a user's profile (which may be partially complete) and
    returns 3-5 recommended tech career tracks with fit scores,
    reasoning, skill overlap analysis, and transition time estimates.
    """
    start_time = time.perf_counter()

    user_id = request_body.user_id
    profile = request_body.profile

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
        from helpers.config import get_llm_config
        config = get_llm_config()

        supervisor_key = (
            getattr(app_settings, "GEMINI_API_KEY_SUPERVISOR", "") 
            or app_settings.GEMINI_API_KEY
        )
        llm = GeminiProvider(
            api_key=supervisor_key,
            max_output_tokens=8192,
            temperature=0.1,
        )
        llm.set_generation_model("gemini-2.5-flash-lite")
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
        import asyncio
        final_state = await asyncio.wait_for(app.ainvoke(initial_state), timeout=150.0)
    except asyncio.TimeoutError:
        logger.error("[TrackRecommend] Graph execution timed out after 150 seconds.")
        return JSONResponse(
            status_code=status.HTTP_504_GATEWAY_TIMEOUT,
            content={
                "signal": "504_Gateway_Timeout",
                "message": "Track recommendation request timed out after 150 seconds.",
            },
        )
    except Exception as exc:
        logger.error(f"[TrackRecommend] Graph execution failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Track recommendation error: {exc}",
            },
        )

    # ── Check for agent errors in final state ────────────────────────
    state_error = final_state.get("error")
    recommendations = final_state.get("track_recommendations")

    if state_error or recommendations is None:
        logger.error(
            f"[TrackRecommend] Agent failed for user={user_id}. "
            f"error={state_error!r} | recommendations={recommendations!r}"
        )
        return JSONResponse(
            status_code=status.HTTP_502_BAD_GATEWAY,
            content={
                "signal":  "502_Bad_Gateway",
                "status":  "error",
                "message": (
                    state_error
                    or "All AI providers are unavailable or rate-limited. "
                       "No recommendations could be generated."
                ),
                "time_consumed": time.perf_counter() - start_time,
            },
        )

    # ── Return success response ───────────────────────────────────────
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
