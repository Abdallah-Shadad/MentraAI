"""
ProjectAgent/agents/schemas/ProjectRecommender.py
===================================================
Pydantic output schema for the ProjectRecommender agent.

Each project recommendation includes a description, the technologies
used, difficulty level, estimated duration, which stages it covers,
and clear milestones that can be split across learning stages.
"""

from typing import List, Optional, Annotated
from pydantic import BaseModel, Field


class ProjectMilestone(BaseModel):
    """A milestone within a project that maps to a specific learning stage."""
    milestone_id: Annotated[str, Field(
        description="Unique identifier for this milestone, e.g. 'ms_1', 'ms_2'"
    )]
    title: Annotated[str, Field(
        description="Short title of this milestone, e.g. 'Set up project scaffold'"
    )]
    description: Annotated[str, Field(
        description="Detailed description of what the learner builds in this milestone"
    )]
    mapped_stage_id: Annotated[str, Field(
        description="The curriculum stage ID this milestone aligns with, e.g. 'stage_1'"
    )]
    skills_applied: Annotated[List[str], Field(
        description="List of skills/topics from that stage used in this milestone"
    )]
    deliverables: Annotated[List[str], Field(
        description="Concrete outputs the learner produces, e.g. 'Landing page HTML', 'REST API endpoint'"
    )]
    estimated_hours: Annotated[int, Field(
        description="Estimated hours to complete this milestone"
    )]


class ProjectRecommendation(BaseModel):
    """A single project recommendation with full details."""
    project_id: Annotated[str, Field(
        description="Unique identifier for this project, e.g. 'proj_1'"
    )]
    title: Annotated[str, Field(
        description="Descriptive project title, e.g. 'Full-Stack E-Commerce Dashboard'"
    )]
    description: Annotated[str, Field(
        description="2-3 sentence overview of what the project involves and why it matters for the job market"
    )]
    project_type: Annotated[str, Field(
        description="Category: 'portfolio_piece', 'capstone', 'mini_project', 'open_source_contribution'"
    )]
    difficulty: Annotated[str, Field(
        description="Overall difficulty: 'beginner', 'intermediate', 'advanced'"
    )]
    technologies: Annotated[List[str], Field(
        description="Technologies and tools used, e.g. ['React', 'Node.js', 'PostgreSQL', 'Docker']"
    )]
    market_relevance: Annotated[str, Field(
        description="Why this project is valuable in the current job market — mention specific roles or industry demand"
    )]
    covers_stages: Annotated[List[str], Field(
        description="List of stage IDs this project spans, e.g. ['stage_1', 'stage_2', 'stage_3']"
    )]
    milestones: Annotated[List[ProjectMilestone], Field(
        description="Ordered list of milestones that break the project into stage-aligned chunks"
    )]
    estimated_total_hours: Annotated[int, Field(
        description="Total estimated hours across all milestones"
    )]
    portfolio_tips: Annotated[List[str], Field(
        description="Tips for showcasing this project in a portfolio or resume, e.g. 'Deploy to Vercel', 'Add CI/CD pipeline'"
    )]


class ProjectRecommenderOutput(BaseModel):
    """Complete output of the ProjectRecommender agent."""
    career_track: Annotated[str, Field(
        description="The career track these projects are tailored for"
    )]
    current_stage_id: Annotated[str, Field(
        description="The stage ID after which these projects are recommended"
    )]
    recommendation_mode: Annotated[str, Field(
        description="'after_stage' = projects for the just-completed stage, 'multi_stage' = project split across upcoming stages, 'both' = includes both types"
    )]
    projects: Annotated[List[ProjectRecommendation], Field(
        description="List of recommended projects (1-3 projects)"
    )]
    summary: Annotated[str, Field(
        description="A brief motivational summary explaining why these projects were chosen and how they align with job market demands"
    )]
