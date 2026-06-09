from typing import List, Optional, Annotated
from pydantic import BaseModel, Field


class TrackMatch(BaseModel):
    """A single tech track recommendation with reasoning and fit score."""

    track_name: Annotated[
        str,
        Field(description="Name of the tech career track (e.g. 'Backend Engineering', 'DevOps / SRE', 'Cloud Architect')"),
    ]
    fit_score: Annotated[
        int,
        Field(
            description="How well the user fits this track (0-100). Based on current skills overlap, experience, and interest signals.",
            ge=0,
            le=100,
        ),
    ]
    reasoning: Annotated[
        str,
        Field(description="2-3 sentence explanation of why this track suits the user."),
    ]
    skill_overlap: Annotated[
        List[str],
        Field(description="User's current skills that directly apply to this track."),
    ]
    skills_to_learn: Annotated[
        List[str],
        Field(description="Key skills the user would need to acquire for this track."),
    ]
    estimated_transition_weeks: Annotated[
        int,
        Field(description="Rough estimate of weeks needed to become job-ready in this track."),
    ]


class TrackRecommenderOutput(BaseModel):
    """Structured output of the TrackRecommender agent."""

    user_summary: Annotated[
        str,
        Field(description="Brief 2-3 sentence summary of the user's profile, strengths, and career signals."),
    ]
    recommended_tracks: Annotated[
        List[TrackMatch],
        Field(
            description="Top 3-5 recommended tech tracks, ordered by fit_score descending.",
            min_length=3,
            max_length=5,
        ),
    ]
    primary_recommendation: Annotated[
        str,
        Field(description="The single best-fit track name with a one-sentence justification."),
    ]
    profile_completeness: Annotated[
        int,
        Field(
            description="How complete the user's profile data is (0-100). Helps the caller know if more info would refine results.",
            ge=0,
            le=100,
        ),
    ]
    missing_info_suggestions: Annotated[
        Optional[List[str]],
        Field(description="Fields that, if provided, would improve recommendation accuracy (e.g. 'years of experience', 'preferred work style')."),
    ]
