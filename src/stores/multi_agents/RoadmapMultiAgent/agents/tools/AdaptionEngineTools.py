"""
adaption_tools.py
─────────────────
Tool 4.1 — search_remedial_resources

Architecture:
  ┌─────────────────────────────────┐
  │      search_remedial_resources  │  ← LangChain @tool (orchestrator)
  └────────────┬────────────────────┘
               │
       ┌───────┴────────┐
       ▼                ▼
  _fetch_tavily    _fetch_youtube      ← pure fetch functions (no @tool)
       │                │
       └───────┬────────┘
               ▼
          Resource (Pydantic model)   ← shared canonical type
"""

from __future__ import annotations

import json
import logging
from typing import Any

import httpx
from langchain_core.tools import tool
from pydantic import BaseModel, Field

from helpers.config import get_settings
import time

logger = logging.getLogger("uvicorn.error")
settings = get_settings()

# ── Canonical Resource Type ────────────────────────────────────────────────────
class Resource(BaseModel):
    """Canonical resource object used across the entire learning platform."""

    title:        str = Field(...,              description="Human-readable title")
    url:          str = Field(...,              description="Direct link to the resource")
    source:       str = Field(...,              description="'youtube' | 'article' | 'docs' …")
    duration_min: int = Field(default=0,        description="Estimated read/watch time in minutes")
    difficulty:   str = Field(default="beginner", description="beginner | intermediate | advanced")
    topic:        str = Field(...,              description="The topic this resource covers")
    why:          str = Field(default="",       description="One-sentence reason this was chosen")


# ── Private Fetchers ───────────────────────────────────────────────────────────

def _fetch_tavily(query: str, topic: str, difficulty: str) -> list[dict]:
    """
    Fetch article results from the Tavily Search API.

    Returns a list of Resource dicts, or an empty list on failure.
    Raises nothing — caller decides how to handle an empty result.
    """
    if not settings.TAVILY_API_KEY:
        logger.info("TAVILY_API_KEY not set — skipping article search.")
        return []

    resp = httpx.post(
        "https://api.tavily.com/search",
        json={
            "api_key": settings.TAVILY_API_KEY,
            "query": query,
            "search_depth": "basic",
            "max_results": 4,
            "include_answer": False,
        },
        timeout=15.0,
    )
    resp.raise_for_status()
    items = resp.json().get("results", [])
    return [
        Resource(
            title=item.get("title", "Untitled"),
            url=item.get("url", ""),
            source="article",
            duration_min=5,
            difficulty=difficulty,
            topic=topic,
            why=f"Top search result for '{topic}' {difficulty}s tutorial",
        ).model_dump()
        for item in items
    ]


def _fetch_youtube(query: str, topic: str, difficulty: str) -> list[dict]:
    """
    Fetch video results from the YouTube Data API v3.

    Returns a list of Resource dicts, or an empty list on failure.
    Raises nothing — caller decides how to handle an empty result.
    """
    if not settings.YOUTUBE_API_KEY:
        logger.info("YOUTUBE_API_KEY not set — skipping video search.")
        return []

    resp = httpx.get(
        "https://www.googleapis.com/youtube/v3/search",
        params={
            "key": settings.YOUTUBE_API_KEY,
            "q": query,
            "part": "snippet",
            "type": "video",
            "maxResults": 3,
            "videoDuration": "medium",     # 4–20 min — ideal for tutorials
            "relevanceLanguage": "en",
            "safeSearch": "strict",
        },
        timeout=15.0,
    )
    resp.raise_for_status()
    items = resp.json().get("items", [])
    return [
        Resource(
            title=item["snippet"].get("title", "Untitled Video"),
            url=f"https://www.youtube.com/watch?v={item['id']['videoId']}",
            source="youtube",
            duration_min=1,                # rough estimate; refine with videos.list
            difficulty=difficulty,
            topic=topic,
            why=f"Beginner YouTube tutorial for '{topic}'",
        ).model_dump()
        for item in items
    ]


# ── Orchestrator ───────────────────────────────────────────────────────────────

class SearchRemedialInput(BaseModel):
    topic:      str = Field(...,               description="Exact topic the user is struggling with")
    difficulty: str = Field(default="beginner", description="beginner | intermediate")


@tool(args_schema=SearchRemedialInput)
def search_remedial_resources(topic: str, difficulty: str = "beginner") -> dict[str, Any]:
    """
    Find beginner-friendly remedial content for a topic the student failed.

    SOURCE : Tavily Search API (articles/docs) + YouTube Data API v3 (videos).
    QUERY  : "{topic} explained simply for beginners tutorial"
    RETURNS: dict with 'status', 'topic', 'resources' (list of Resource dicts).

    Tavily runs first. YouTube is only called if Tavily fails.
    Use this FIRST after receiving the weak topic from the supervisor.
    """
    query = f"{topic} explained simply for {difficulty}s tutorial"
    resources: list[dict] = []

    # ── Tavily (articles) — YouTube is the fallback if this fails ────────────
    try:
        tavily_results = _fetch_tavily(query, topic, difficulty)
        resources.extend(tavily_results)
        logger.info("Tavily returned %d results.", len(tavily_results))
    except Exception as exc:
        logger.warning("Tavily fetch failed (%s) — falling back to YouTube.", exc)
        try:
            youtube_results = _fetch_youtube(query, topic, difficulty)
            resources.extend(youtube_results)
            logger.info("YouTube fallback returned %d results.", len(youtube_results))
        except Exception as yt_exc:
            logger.warning("YouTube fallback also failed: %s", yt_exc)

    # ── Response ───────────────────────────────────────────────────────────────
    if not resources:
        return {
            "status": "no_results",
            "topic": topic,
            "difficulty":"NULL",
            "resource_count":0,
            "message": (
                f"No remedial resources found for '{topic}'. "
                "Check API keys or try a broader topic name."
            ),
            "resources": [],
        }

    return {
        "status": "success",
        "topic": topic,
        "difficulty": difficulty,
        "resource_count": len(resources),
        "message":"Research Done Successfully!!",
        "resources": resources,
    }


# ── Registered Tools ───────────────────────────────────────────────────────────

ADAPTION_TOOLS = [search_remedial_resources]


# ── Smoke Tests ────────────────────────────────────────────────────────────────
def _test_search_remedial_resources() -> None:
    result = search_remedial_resources.invoke({
        "topic": "Python list comprehensions",
        "difficulty": "beginner",
    })
    print(json.dumps(result, indent=2))

    assert "status"    in result,                    "FAIL: missing 'status'"
    assert "resources" in result,                    "FAIL: missing 'resources'"
    assert isinstance(result["resources"], list),    "FAIL: 'resources' must be a list"
    if result["status"] == "success":
        assert len(result["resources"]) > 0, "FAIL: resource_count must be > 0"
        print("\n✓ Tool 4.1 structure check PASSED")
    else:
        assert result["status"] == "no_results", "FAIL: status must be 'no_results'"
        print("\nX Tool 4.1 structure check PASSED (No results found)")

if __name__ == "__main__":
    print("=" * 60)
    print("  AdaptionEngine — Tool Registration + Smoke Tests")
    print("=" * 60)
    print(f"\n✓ Tools registered: {len(ADAPTION_TOOLS)}")
    for t in ADAPTION_TOOLS:
        print(f"  • {t.name}")

    _test_search_remedial_resources()