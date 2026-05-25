"""
MentorContext — Learner State Model & Prompt Builder
=====================================================

Holds all the optional context the frontend sends per request, and builds
a structured prompt block that is PREPENDED to the LLM tier prompt so the
model always knows who it is talking to.

All mentor fields are Optional — a brand-new user with no progress data
simply gets the generic tier prompt with no learner block injected.
"""

from __future__ import annotations

from typing import Optional
from pydantic import BaseModel, Field


# ── Score thresholds ────────────────────────────────────────────────────
_SCORE_STRONG    = 85   # 85-100 → mastered
_SCORE_GOOD      = 70   # 70-84  → solid, minor gaps
_SCORE_WEAK      = 50   # 50-69  → needs reinforcement
# below 50           → struggling — refocus explanation


class MentorContext(BaseModel):
    """
    Captures everything the frontend knows about the learner at request time.

    Required:
        user_id         — identifies the user in Redis memory
        conversation_id — scopes the conversation history bucket

    All other fields are optional.  The frontend sends them progressively
    as the user makes progress; before any progress is recorded they arrive
    as null / absent.
    """

    user_id:         str = Field(..., description="Unique user identifier.")
    conversation_id: str = Field(..., description="Unique conversation bucket.")

    # ── Optional learner state ──────────────────────────────────────────
    career_track: Optional[str] = Field(
        None,
        description="The learner's chosen career path, e.g. 'backend', 'frontend', 'data'.",
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
        description="Most recent quiz score (0-100). None = no quiz taken yet.",
    )

    @property
    def has_mentor_data(self) -> bool:
        """True if at least one optional mentor field is present."""
        return any([
            self.career_track,
            self.stage,
            self.lesson_id,
            self.quiz_details,
            self.quiz_score is not None,
        ])


# ── Prompt builder ──────────────────────────────────────────────────────

def _score_label(score: int) -> str:
    """Return a short label + emoji for a quiz score."""
    if score >= _SCORE_STRONG:
        return f"{score}/100 ✅ (mastered)"
    if score >= _SCORE_GOOD:
        return f"{score}/100 👍 (solid, minor gaps)"
    if score >= _SCORE_WEAK:
        return f"{score}/100 ⚠️ (needs reinforcement)"
    return f"{score}/100 ❌ (struggling — refocus explanation)"


def build_mentor_prefix(ctx: MentorContext) -> str:
    """
    Build a system-prompt block that describes the learner's current state.

    Returns an empty string when no mentor data is present (new user),
    so the caller can safely concatenate without any guard.

    This block is PREPENDED to the tier prompt (simple / medium / advanced)
    chosen by the classifier.

    Example output
    ──────────────
    [LEARNER PROFILE]
    Career Track : backend
    Current Stage: advanced_python
    Active Lesson: decorators_intro
    Quiz         : quiz_title+quiz_lesson — Score: 60/100 ⚠️ (needs reinforcement)

    You are a personalized AI mentor. Tailor every answer to this learner's
    exact position in their curriculum. Reinforce the active lesson concept
    and address any knowledge gaps revealed by the quiz score above.
    ──────────────────────────────────────────────────────────────────────
    """
    if not ctx.has_mentor_data:
        return ""

    lines: list[str] = ["[LEARNER PROFILE]"]

    if ctx.career_track:
        lines.append(f"Career Track : {ctx.career_track}")
    if ctx.stage:
        lines.append(f"Current Stage: {ctx.stage}")
    if ctx.lesson_id:
        lines.append(f"Active Lesson: {ctx.lesson_id}")
    if ctx.quiz_details and ctx.quiz_score is not None:
        lines.append(
            f"Quiz         : {ctx.quiz_details} — Score: {_score_label(ctx.quiz_score)}"
        )
    elif ctx.quiz_details:
        lines.append(f"Quiz         : {ctx.quiz_details} — Score: not yet taken")
    elif ctx.quiz_score is not None:
        lines.append(f"Quiz Score   : {_score_label(ctx.quiz_score)}")

    profile_block = "\n".join(lines)

    # ── Behavioural instruction ──────────────────────────────────────────
    instruction_lines = [
        "",
        "You are a personalized AI mentor. Tailor every answer to this learner's",
        "exact position in their curriculum. Reinforce the active lesson concept.",
    ]

    # Add score-specific instruction only when we have a score
    if ctx.quiz_score is not None:
        if ctx.quiz_score < _SCORE_WEAK:
            instruction_lines.append(
                "The learner is struggling — simplify your explanation, use analogies, "
                "and correct likely misconceptions before moving forward."
            )
        elif ctx.quiz_score < _SCORE_GOOD:
            instruction_lines.append(
                "The learner needs reinforcement — address gaps, provide examples, "
                "and confirm understanding before introducing new concepts."
            )
        elif ctx.quiz_score < _SCORE_STRONG:
            instruction_lines.append(
                "The learner has a solid grasp — acknowledge their progress and gently "
                "challenge them with slightly deeper nuance."
            )
        else:
            instruction_lines.append(
                "The learner has mastered this — feel free to introduce advanced "
                "edge cases or related concepts to extend their understanding."
            )

    mentor_instruction = "\n".join(instruction_lines)

    return f"{profile_block}{mentor_instruction}\n\n"
