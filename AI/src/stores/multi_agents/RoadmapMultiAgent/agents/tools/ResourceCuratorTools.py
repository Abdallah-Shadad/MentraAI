"""
ResourceCuratorTools.py
───────────────────────
Tools for finding high-quality learning resources (articles and videos).
"""

from __future__ import annotations

import logging
import time
from typing import Any

import httpx
from langchain_core.tools import tool
from pydantic import BaseModel, Field

from helpers.config import get_settings

logger = logging.getLogger("uvicorn.error")
settings = get_settings()

class Resource(BaseModel):
    title:        str = Field(...,               description="Human-readable title")
    url:          str = Field(...,               description="Direct link to the resource")
    source:       str = Field(...,               description="'youtube' | 'article' | 'docs' …")
    duration_min: int = Field(default=0,         description="Estimated read/watch time in minutes")
    difficulty:   str = Field(default="beginner", description="beginner | intermediate | advanced")
    topic:        str = Field(...,               description="The topic this resource covers")
    why:          str = Field(default="",        description="One-sentence reason this was chosen")


def _with_retry(fn, *args, max_retries: int = 2, **kwargs):
    """
    Call fn(*args, **kwargs) with simple exponential backoff on HTTP 429.
    Returns the response or raises on final failure.
    """
    for attempt in range(max_retries + 1):
        try:
            resp = fn(*args, **kwargs)
            resp.raise_for_status()
            return resp
        except httpx.HTTPStatusError as exc:
            if exc.response.status_code == 429 and attempt < max_retries:
                wait = 2 ** attempt          # 1 s, 2 s
                logger.warning(
                    "Rate-limited (429) on attempt %d/%d — retrying in %ds.",
                    attempt + 1, max_retries + 1, wait,
                )
                time.sleep(wait)
            else:
                raise
    raise RuntimeError("Unreachable")


def _fetch_tavily(query: str, topic: str, difficulty: str) -> list[dict]:
    if not settings.TAVILY_API_KEY:
        logger.info("TAVILY_API_KEY not set — skipping article search.")
        return []

    try:
        resp = _with_retry(
            httpx.post,
            "https://api.tavily.com/search",
            json={
                "api_key": settings.TAVILY_API_KEY,
                "query": query,
                "search_depth": "basic",
                "max_results": 5,           # 5 candidates — enough to pick the best article
                "include_answer": False,
            },
            timeout=20.0,
        )
    except Exception as exc:
        logger.warning("Tavily fetch failed: %s", exc)
        return []

    items = resp.json().get("results", [])
    return [
        Resource(
            title=item.get("title", "Untitled"),
            url=item.get("url", ""),
            source="article",
            duration_min=7,
            difficulty=difficulty,
            topic=topic,
            why=f"Top article for '{topic}' at {difficulty} level",
        ).model_dump()
        for item in items
    ]


def _fetch_youtube(query: str, topic: str, difficulty: str) -> list[dict]:
    if not settings.YOUTUBE_API_KEY:
        logger.info("YOUTUBE_API_KEY not set — skipping video search.")
        return []

    try:
        resp = _with_retry(
            httpx.get,
            "https://www.googleapis.com/youtube/v3/search",
            params={
                "key":              settings.YOUTUBE_API_KEY,
                "q":                query,
                "part":             "snippet",
                "type":             "video",
                "maxResults":       5,       # 5 candidates — enough to pick the best video
                "videoDuration":    "medium", # 4–20 min — ideal for tutorials
                "relevanceLanguage": "en",
                "safeSearch":       "strict",
            },
            timeout=20.0,
        )
    except Exception as exc:
        logger.warning("YouTube fetch failed: %s", exc)
        return []

    items = resp.json().get("items", [])
    return [
        Resource(
            title=item["snippet"].get("title", "Untitled Video"),
            url=f"https://www.youtube.com/watch?v={item['id']['videoId']}",
            source="youtube",
            duration_min=10,
            difficulty=difficulty,
            topic=topic,
            why=f"YouTube tutorial for '{topic}'",
        ).model_dump()
        for item in items
    ]


class SearchLearningInput(BaseModel):
    topic:      str = Field(...,                description="Exact topic to find resources for")
    difficulty: str = Field(default="beginner", description="beginner | intermediate | advanced")


@tool(args_schema=SearchLearningInput)
def search_learning_resources(topic: str, difficulty: str = "beginner") -> dict[str, Any]:
    """
    Find high-quality learning resources for a given topic.

    SOURCE : Tavily Search API (articles/docs) + YouTube Data API v3 (videos).
    QUERY  : "{topic} tutorial for {difficulty}s"
    RETURNS: dict with 'status', 'topic', 'resources' (list of Resource dicts).
             Returns up to 5 article candidates and 5 video candidates.
             The agent selects the single best video and single best article from these.
    """
    query = f"{topic} tutorial for {difficulty}s"
    resources: list[dict] = []

    tavily_results = _fetch_tavily(query, topic, difficulty)
    resources.extend(tavily_results)

    youtube_results = _fetch_youtube(query, topic, difficulty)
    resources.extend(youtube_results)

    if not resources:
        return {
            "status": "no_results",
            "topic": topic,
            "difficulty": difficulty,
            "resource_count": 0,
            "resources": [],
        }

    return {
        "status": "success",
        "topic": topic,
        "difficulty": difficulty,
        "resource_count": len(resources),
        "resources": resources,
    }


RESOURCE_TOOLS = [search_learning_resources]
