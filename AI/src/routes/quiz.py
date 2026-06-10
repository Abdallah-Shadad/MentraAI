"""
routes/quiz.py
===============
Quiz API endpoint for generating quiz questions with correct answers and hints.

The AI server is responsible ONLY for question generation.
The backend server handles:
  - Serving questions to the frontend (hiding answers/hints)
  - Evaluating submitted answers using the correct_answer data
  - Serving hints progressively based on hint_level
  - Grading and scoring

POST /api/v1/quiz/generate  — Generate quiz questions for a curriculum stage
"""

import logging
import time
from fastapi import APIRouter, Depends, Request, status
from fastapi.responses import JSONResponse
from fastapi.encoders import jsonable_encoder

from helpers.config import get_settings, Settings
from stores.multi_agents.QuizAgent.QuizGraph import QuizGraph, QuizState
from stores.multi_agents.AgentProviderFactory import AgentProviderFactory
from stores.llm.providers.GeminiProvider import GeminiProvider

logger = logging.getLogger("uvicorn.error")

# ── Router ─────────────────────────────────────────────────────────────────────

quiz_router = APIRouter(
    prefix="/api/v1/quiz",
    tags=["quiz"],
)


# ── Helpers ────────────────────────────────────────────────────────────────────

def _build_llm_and_config(app_settings: Settings):
    """
    Builds the LLM provider from application settings.
    Mirrors the pattern used in routes/roadmap.py.
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
# ENDPOINT — Generate Quiz Questions
# ══════════════════════════════════════════════════════════════════════════════

@quiz_router.post("/generate", status_code=status.HTTP_201_CREATED)
async def generate_quiz(
    request: Request,
    app_settings: Settings = Depends(get_settings),
):
    """
    POST /api/v1/quiz/generate

    Generates quiz questions for a specific curriculum stage. Each question
    includes correct answers, explanations, and progressive hints.

    The backend server receives the full quiz payload and can:
    - Strip correct_answer/explanation/hints before sending to frontend
    - Use correct_answer to evaluate learner submissions
    - Serve hints progressively (level 1 → 2 → 3) on frontend request

    Request body
    ------------
    {
        "user_id": "user_123",
        "career_track": "Web Development",
        "stage_id": "stage_1",
        "stage_name": "HTML5 Fundamentals",
        "topics": ["HTML5 Tags", "Semantic HTML", "Forms"],
        "learning_objectives": {"Build a form": "Create an accessible HTML form"},
        "difficulty_level": "beginner"
    }

    Response data.quiz structure
    ----------------------------
    {
        "stage_id": "stage_1",
        "topic": "HTML5 Fundamentals",
        "difficulty_level": "beginner",
        "total_questions": 5,
        "time_limit_minutes": 10,
        "passing_score": 70,
        "questions": [
            {
                "question_id": "q_1",
                "question_text": "...",
                "question_type": "multiple_choice",
                "difficulty": "easy",
                "bloom_level": "understand",
                "topic": "Semantic HTML",
                "choices": [
                    {"label": "A", "text": "...", "is_correct": false},
                    {"label": "B", "text": "...", "is_correct": true},
                    ...
                ],
                "correct_answer": "B",
                "explanation": "...",
                "hints": [
                    {"level": 1, "text": "subtle nudge..."},
                    {"level": 2, "text": "moderate guidance..."},
                    {"level": 3, "text": "strong clue..."}
                ]
            }
        ]
    }
    """
    start_time = time.perf_counter()

    # ── Parse request body ────────────────────────────────────────────────
    try:
        data = await request.json()
    except Exception as exc:
        logger.error(f"[QuizGenerate] Failed to parse request body: {exc}")
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
        f"[QuizGenerate] user={data['user_id']} | stage={data['stage_id']} | "
        f"topics={data['topics']}"
    )

    # ── Build initial state ───────────────────────────────────────────────
    initial_state: QuizState = {
        "user_id":              data["user_id"],
        "career_track":         data["career_track"],
        "stage_id":             data["stage_id"],
        "stage_name":           data.get("stage_name", data["stage_id"]),
        "topics":               data["topics"],
        "learning_objectives":  data.get("learning_objectives", {}),
        "difficulty_level":     data.get("difficulty_level", "beginner"),
    }

    # ── Build LLM + Graph ─────────────────────────────────────────────────
    try:
        llm, config = _build_llm_and_config(app_settings)
        agent_factory = AgentProviderFactory(config)

        graph = QuizGraph(
            config=config,
            agent_factory=agent_factory,
            llm=llm,
        )
        app = graph.build().compile()
    except Exception as exc:
        logger.error(f"[QuizGenerate] Graph initialisation failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Quiz graph initialisation error: {exc}",
            },
        )

    # ── Invoke graph ──────────────────────────────────────────────────────
    try:
        final_state = app.invoke(initial_state)
    except Exception as exc:
        logger.error(f"[QuizGenerate] Graph execution failed: {exc}")
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={
                "signal": "500_Internal_Server_Error",
                "message": f"Quiz generation error: {exc}",
            },
        )

    return JSONResponse(
        status_code=status.HTTP_201_CREATED,
        content=jsonable_encoder({
            "signal":  "201_Created",
            "status":  "success",
            "message": "Quiz generated successfully",
            "quiz":    final_state.get("api_response", {}).get("data", {}).get("quiz", {}),
            "time_consumed": time.perf_counter() - start_time,
        }),
    )
