"""
routes/project.py
==================
Project Recommendation API endpoint for generating job-market-relevant
project recommendations after each learning stage.

The AI server generates project recommendations with milestones mapped
to curriculum stages. The backend server can then:
  - Display recommended projects to the learner after completing a stage
  - Track project progress across milestones
  - Use portfolio tips for career guidance features

POST /api/v1/projects/recommend  — Generate project recommendations for a stage
"""

import logging
import time
from fastapi import APIRouter, Depends, Request, status
from fastapi.responses import JSONResponse
from fastapi.encoders import jsonable_encoder

from helpers.config import get_settings, Settings
from stores.multi_agents.ProjectAgent.ProjectGraph import ProjectGraph, ProjectState
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
from stores.llm.providers.GeminiProvider import GeminiProvider

logger = logging.getLogger("uvicorn.error")

# ── Router ─────────────────────────────────────────────────────────────────────

project_router = APIRouter(
    prefix="/api/v1/projects",
    tags=["projects"],
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
# ENDPOINT — Generate Project Recommendations
# ══════════════════════════════════════════════════════════════════════════════

@project_router.post("/recommend", status_code=status.HTTP_201_CREATED)
async def recommend_projects(
    request: Request,
    app_settings: Settings = Depends(get_settings),
):
    """
    POST /api/v1/projects/recommend

    Generates project recommendations for a learner based on their current
    curriculum stage and progress. Each project includes milestones mapped
    to specific learning stages, market relevance, and portfolio tips.

    Request body
    ------------
    {
        "user_id": "user_123",
        "career_track": "Web Development",
        "stage_id": "stage_1",
        "stage_name": "HTML5 Fundamentals",
        "topics": ["HTML5 Tags", "Semantic HTML", "Forms"],
        "learning_objectives": {"Build a form": "Create an accessible HTML form"},
        "difficulty_level": "beginner",
        "completed_stages": [
            {"id": "stage_0", "name": "Intro", "topics": ["Basics"]}
        ],
        "upcoming_stages": [
            {"id": "stage_2", "name": "CSS3", "topics": ["Flexbox", "Grid"]}
        ]
    }

    Response data.recommendations structure
    ----------------------------------------
    {
        "career_track": "Web Development",
        "current_stage_id": "stage_1",
        "recommendation_mode": "after_stage",
        "projects": [
            {
                "project_id": "proj_1",
                "title": "Personal Portfolio Website",
                "description": "...",
                "project_type": "portfolio_piece",
                "difficulty": "beginner",
                "technologies": ["HTML5", "Semantic HTML"],
                "market_relevance": "...",
                "covers_stages": ["stage_1"],
                "milestones": [
                    {
                        "milestone_id": "ms_1",
                        "title": "...",
                        "description": "...",
                        "mapped_stage_id": "stage_1",
                        "skills_applied": ["HTML5"],
                        "deliverables": ["Landing page"],
                        "estimated_hours": 3
                    }
                ],
                "estimated_total_hours": 10,
                "portfolio_tips": ["Deploy to GitHub Pages", "..."]
            }
        ],
        "summary": "..."
    }
    """
    start_time = time.perf_counter()

    # ── Parse request body ────────────────────────────────────────────────
    try:
        data = await request.json()
    except Exception as exc:
        logger.error(f"[ProjectRecommend] Failed to parse request body: {exc}")
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"signal": "400_Bad_Request", "message": "Invalid JSON body."},
        )

    # ── Validate required fields ──────────────────────────────────────────
    required = ["user_id", "career_track", "stage_id", "topics"]
    missing = [f for f in required if not data.get(f)]
    if missing:
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={
                "signal": "400_Bad_Request",
                "message": f"Missing required fields: {', '.join(missing)}",
            },
        )

    logger.info(
        f"[ProjectRecommend] user={data['user_id']} | stage={data['stage_id']} | "
        f"topics={data['topics']}"
    )

    # ── Build initial state ───────────────────────────────────────────────
    initial_state: ProjectState = {
        "user_id":              data["user_id"],
        "career_track":         data["career_track"],
        "stage_id":             data["stage_id"],
        "stage_name":           data.get("stage_name", data["stage_id"]),
        "topics":               data["topics"],
        "learning_objectives":  data.get("learning_objectives", {}),
        "difficulty_level":     data.get("difficulty_level", "beginner"),
        "completed_stages":     data.get("completed_stages", []),
        "upcoming_stages":      data.get("upcoming_stages", []),
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

        graph = ProjectGraph(
            config=config,
            agent_factory=agent_factory,
            llm=llm,
        )
        app = graph.build().compile()
    except Exception as exc:
        logger.error(f"[ProjectRecommend] Graph initialisation failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Project graph initialisation error: {exc}",
            },
        )

    # ── Invoke graph ──────────────────────────────────────────────────────
    try:
        import asyncio
        final_state = await asyncio.wait_for(app.ainvoke(initial_state), timeout=150.0)
    except asyncio.TimeoutError:
        logger.error("[ProjectRecommend] Graph execution timed out after 150 seconds.")
        return JSONResponse(
            status_code=status.HTTP_504_GATEWAY_TIMEOUT,
            content={
                "signal": "504_Gateway_Timeout",
                "message": "Project recommendation request timed out after 150 seconds.",
            },
        )
    except Exception as exc:
        logger.error(f"[ProjectRecommend] Graph execution failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Project recommendation error: {exc}",
            },
        )

    # ── Check for agent errors in final state ────────────────────────
    state_error = final_state.get("error")
    project_recommendations = final_state.get("project_recommendations")

    if state_error or project_recommendations is None:
        logger.error(
            f"[ProjectRecommend] Agent failed for user={data['user_id']}. "
            f"error={state_error!r}"
        )
        return JSONResponse(
            status_code=status.HTTP_502_BAD_GATEWAY,
            content={
                "signal":  "502_Bad_Gateway",
                "status":  "error",
                "message": (
                    state_error
                    or "All AI providers are unavailable or rate-limited. "
                       "Project recommendations could not be generated."
                ),
                "time_consumed": time.perf_counter() - start_time,
            },
        )

    return JSONResponse(
        status_code=status.HTTP_201_CREATED,
        content=jsonable_encoder({
            "signal":  "201_Created",
            "status":  "success",
            "message": "Project recommendations generated successfully",
            "projects": final_state.get("api_response", {}),
            "time_consumed": time.perf_counter() - start_time,
        }),
    )
