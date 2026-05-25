from fastapi import APIRouter, Depends, Request, status
from fastapi.responses import JSONResponse, StreamingResponse
from pydantic import BaseModel, Field
from typing import Optional

from helpers.config import get_settings, Settings
from stores.multi_agents.ChatAgent.embedder import Embedder
from stores.multi_agents.ChatAgent.ChatRouter import ChatRouter
from stores.multi_agents.ChatAgent.MentorContext import MentorContext

import logging

logger = logging.getLogger("uvicorn.error")

# ── Singleton — created once; setup() is called at app lifespan startup ──────
_chat_router_instance = ChatRouter()

chat_router = APIRouter(
    prefix="/api/v1/chat",
    tags=["chat"],
)


# ─────────────────────────────────────────────────────────────────
# Request / Response schemas
# ─────────────────────────────────────────────────────────────────

class ChatRequest(BaseModel):
    # ── Required ───────────────────────────────────────────────
    user_id:         str = Field(..., description="Unique identifier for the user.")
    conversation_id: str = Field(..., description="Conversation bucket ID. Same ID continues history; new ID starts fresh.")
    query:           str = Field(..., min_length=1, description="The user's message.")

    # ── Optional learner state (sent progressively as user makes progress) ──
    career_track: Optional[str] = Field(
        None,
        description="Learner's career path, e.g. 'backend', 'frontend', 'data'.",
    )
    stage: Optional[str] = Field(
        None,
        description="Current learning stage, e.g. 'advanced_python', 'django_basics'.",
    )
    lesson_id: Optional[str] = Field(
        None,
        description="Active lesson identifier, e.g. 'decorators_intro'.",
    )
    quiz_details: Optional[str] = Field(
        None,
        description="Quiz label, e.g. 'quiz_title+quiz_lesson'.",
    )
    quiz_score: Optional[int] = Field(
        None,
        ge=0,
        le=100,
        description="Most recent quiz score (0-100). Null = no quiz taken yet.",
    )


class ClearMemoryRequest(BaseModel):
    user_id:         str = Field(..., description="User whose conversation should be cleared.")
    conversation_id: str = Field(..., description="The specific conversation to clear.")


# ─────────────────────────────────────────────────────────────────
# POST /api/v1/chat/
# ─────────────────────────────────────────────────────────────────

@chat_router.post(
    "/",
    summary="Streaming chat",
    description=(
        "Send a message and receive a token-by-token streamed response. "
        "Automatically routes to the appropriate LLM tier (simple / medium / advanced) "
        "and uses per-user Redis memory + semantic caching."
    ),
    response_class=StreamingResponse,
)
async def chat(
    body: ChatRequest,
    app_settings: Settings = Depends(get_settings),
):
    """
    Main chat endpoint.

    Returns a `text/plain` streaming response.
    The client should read the body chunk-by-chunk to display tokens
    as they arrive (Server-Sent Events style or raw stream).

    Response headers:
        X-Cache: HIT   — response was served from the Redis semantic cache.
        X-Cache: MISS  — response was generated live by the LLM.

    On error, returns a standard JSON error response.
    """
    if not body.query.strip():
        return JSONResponse(
            status_code=status.HTTP_400_BAD_REQUEST,
            content={"error": "query must not be empty."},
        )

    # ── Build MentorContext from the request body ────────────────────────
    ctx = MentorContext(
        user_id=body.user_id,
        conversation_id=body.conversation_id,
        career_track=body.career_track,
        stage=body.stage,
        lesson_id=body.lesson_id,
        quiz_details=body.quiz_details,
        quiz_score=body.quiz_score,
    )

    # ── Eagerly resolve cache status BEFORE streaming starts ────────────
    # resolve() awaits both the classifier and the cache check, returning
    # (hit: bool, stream) so we can set X-Cache before any bytes are sent.
    try:
        is_cache_hit, token_stream = await _chat_router_instance.resolve(
            ctx=ctx,
            query=body.query.strip(),
        )
        logger.info(
            "chat — X-Cache: %s  user=%s  conv=%s",
            "HIT" if is_cache_hit else "MISS",
            body.user_id,
            body.conversation_id,
        )
    except Exception as exc:
        logger.error(
            "chat endpoint — resolve error (user=%s  conv=%s): %s",
            body.user_id, body.conversation_id, exc,
        )
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={"error": "Failed to resolve chat request."},
        )

    async def safe_stream():
        try:
            async for chunk in token_stream:
                yield chunk
        except Exception as exc:
            logger.error(
                "chat endpoint — stream error (user=%s  conv=%s): %s",
                body.user_id, body.conversation_id, exc,
            )
            yield "\n[Error: an internal error occurred.]"

    return StreamingResponse(
        safe_stream(),
        media_type="text/plain",
        headers={
            "X-Cache":           "HIT" if is_cache_hit else "MISS",
            "X-Conversation-Id": body.conversation_id,
            "X-Accel-Buffering": "no",       # disable Nginx proxy buffering
            "Cache-Control":     "no-cache", # prevent client-side caching of the stream
        },
    )


# ─────────────────────────────────────────────────────────────────
# DELETE /api/v1/chat/memory
# ─────────────────────────────────────────────────────────────────

@chat_router.delete(
    "/memory",
    summary="Clear conversation memory",
    description=(
        "Wipe Redis conversation history for a specific user + conversation pair. "
        "Other conversations for the same user are NOT affected."
    ),
)
async def clear_memory(body: ClearMemoryRequest):
    try:
        await _chat_router_instance.clear_conversation(
            user_id=body.user_id,
            conversation_id=body.conversation_id,
        )
        return JSONResponse(
            status_code=status.HTTP_200_OK,
            content={
                "message": (
                    f"Memory cleared for user '{body.user_id}' "
                    f"conversation '{body.conversation_id}'."
                )
            },
        )
    except Exception as exc:
        logger.error(
            "clear_memory error (user=%s  conv=%s): %s",
            body.user_id, body.conversation_id, exc,
        )
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={"error": str(exc)},
        )


# ─────────────────────────────────────────────────────────────────
# GET /api/v1/chat/health
# ─────────────────────────────────────────────────────────────────

@chat_router.get(
    "/health",
    summary="Chat subsystem health check",
    description="Returns Redis cache + memory health status.",
)
async def health():
    result = await _chat_router_instance.health_check()
    ok = all(v == "ok" for v in result.values())
    return JSONResponse(
        status_code=status.HTTP_200_OK if ok else status.HTTP_503_SERVICE_UNAVAILABLE,
        content=result,
    )


# ─────────────────────────────────────────────────────────────────
# POST /api/v1/chat/embedder_test  (existing, unchanged)
# ─────────────────────────────────────────────────────────────────

@chat_router.post("/embedder_test")
async def embedding_test(
    request: Request,
    app_settings: Settings = Depends(get_settings),
):
    try:
        data = await request.json()
        text = data.get("text")
        if not text:
            return JSONResponse(
                status_code=status.HTTP_400_BAD_REQUEST,
                content={"message": "text is required"},
            )
        embedder  = Embedder()
        embedding = await embedder.get_embedding(text)
        return JSONResponse(
            status_code=status.HTTP_200_OK,
            content={"embedding": embedding},
        )
    except Exception as e:
        logger.error("Error generating embedding: %s", e)
        return JSONResponse(
            status_code=status.HTTP_500_INTERNAL_SERVER_ERROR,
            content={"error": f"Error generating embedding: {e}"},
        )


# ─────────────────────────────────────────────────────────────────
# Lifespan helper — called by main.py at startup
# ─────────────────────────────────────────────────────────────────

async def init_chat_router() -> None:
    """
    Run once at application startup.
    Creates the Redis vector search index for the semantic cache.

    If Redis is unreachable the server starts in degraded mode:
    cache and memory are disabled but the LLM routing still works.
    """
    try:
        await _chat_router_instance.setup()
        logger.info("ChatRouter — Redis vector index ready.")
    except Exception as exc:
        logger.warning(
            "ChatRouter — Redis unavailable at startup (%s). "
            "Server running in DEGRADED MODE: semantic cache and conversation "
            "memory are disabled until Redis is reachable.",
            exc,
        )