"""
RoadmapMultiAgent/agents/schemas/AdaptationEngine.py
=====================================================
Output schema for the AdaptationEngine agent.

This Pydantic model is passed to ``with_structured_output`` so the LLM is
forced to return a well-typed, validated object every time.

Schema maps 1-to-1 with the system prompt's expected JSON so the LLM
never has to guess field names.
"""

from typing import List, Optional, Annotated
from pydantic import BaseModel, Field


# ── Sub-models ─────────────────────────────────────────────────────────────────

class RemediationResource(BaseModel):
    """A single learning resource recommended to fix a knowledge gap."""
    title:        str = Field(..., description="Human-readable title of the resource")
    url:          str = Field(..., description="Direct URL to the resource")
    source:       str = Field(..., description="'youtube' | 'article' | 'docs' | 'course'")
    duration_min: int = Field(default=0, description="Estimated read/watch time in minutes")
    difficulty:   str = Field(default="beginner", description="beginner | intermediate | advanced")
    topic:        str = Field(..., description="The exact sub-topic this resource covers")
    why:          str = Field(default="", description="One sentence explaining why this resource was selected")


class FailedQuestion(BaseModel):
    """Structured breakdown of a single incorrectly answered question."""
    question_id:    str = Field(..., description="Question ID matching the quiz payload")
    question_text:  str = Field(..., description="The question text")
    correct_answer: str = Field(..., description="The correct answer")
    user_answer:    str = Field(..., description="What the learner answered")
    topic_gap:      str = Field(..., description="The knowledge gap this wrong answer reveals")


class StageAdjustment(BaseModel):
    """A concrete change to make to the stage or roadmap."""
    action:   str = Field(..., description="'insert_remedial' | 'reorder' | 'extend_stage' | 'add_prerequisite'")
    stage_id: str = Field(..., description="The stage this adjustment targets")
    reason:   str = Field(..., description="One-sentence explanation of why this change is needed")


# ── Root Output Model ──────────────────────────────────────────────────────────

class AdaptationEngineOutput(BaseModel):
    """
    Full output of the AdaptationEngine agent.

    Written to state key ``adapted_curriculum``.
    Consumed by ResponseFormatter to build the API response.
    """

    # ── Quiz analysis ──────────────────────────────────────────────────────
    stage_id:           str = Field(..., description="ID of the stage being adapted (e.g. 'stage_0')")
    topic:              str = Field(..., description="Primary topic the learner failed on")
    score:              int = Field(..., description="The learner's quiz score as a percentage (always < 50)")
    failed_questions:   List[FailedQuestion] = Field(
        default_factory=list,
        description="Breakdown of each incorrectly answered question and the gap it reveals"
    )
    struggling_topics:  List[str] = Field(
        ...,
        description="Ordered list of specific sub-topics the learner needs to revisit most urgently"
    )

    # ── Remedial resources (from search_remedial_resources tool) ────────────
    remedial_resources: List[RemediationResource] = Field(
        default_factory=list,
        description="Resources fetched by the search_remedial_resources tool for each struggling topic"
    )

    # ── Stage adjustments ──────────────────────────────────────────────────
    stage_adjustments:  List[StageAdjustment] = Field(
        default_factory=list,
        description="Concrete structural changes recommended for the roadmap"
    )

    # ── Summary ────────────────────────────────────────────────────────────
    summary:            str = Field(
        ...,
        description="2-3 sentence plain-English explanation of what was found and what was changed"
    )
    recommended_next_action: str = Field(
        default="retry_stage",
        description="'retry_stage' | 'review_resources' | 'seek_mentor' — what the learner should do next"
    )
