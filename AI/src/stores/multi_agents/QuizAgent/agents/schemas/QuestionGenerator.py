"""
QuizAgent/agents/schemas/QuestionGenerator.py
==============================================
Pydantic output schema for the QuestionGenerator agent.

Each quiz question includes the question text, answer choices,
the correct answer, hints (progressive difficulty), and metadata
like difficulty level and Bloom's taxonomy level.
"""

from typing import List, Optional, Annotated
from pydantic import BaseModel, Field


class AnswerChoice(BaseModel):
    """A single answer option in a multiple-choice question."""
    label: Annotated[str, Field(
        description="Choice label, e.g. 'A', 'B', 'C', 'D'"
    )]
    text: Annotated[str, Field(
        description="The text content of this answer choice"
    )]
    is_correct: Annotated[bool, Field(
        description="Whether this choice is the correct answer"
    )]


class Hint(BaseModel):
    """A progressive hint that helps the learner without giving away the answer."""
    level: Annotated[int, Field(
        description="Hint difficulty level: 1 = subtle nudge, 2 = moderate guidance, 3 = strong clue"
    )]
    text: Annotated[str, Field(
        description="The hint text content"
    )]


class QuizQuestion(BaseModel):
    """A single quiz question with answer choices, correct answer, and hints."""
    question_id: Annotated[str, Field(
        description="Unique identifier for this question, e.g. 'q_1', 'q_2'"
    )]
    question_text: Annotated[str, Field(
        description="The question text presented to the learner"
    )]
    question_type: Annotated[str, Field(
        description="Type of question: 'multiple_choice', 'true_false', 'fill_in_blank'"
    )]
    difficulty: Annotated[str, Field(
        description="Difficulty level: 'easy', 'medium', 'hard'"
    )]
    bloom_level: Annotated[str, Field(
        description="Bloom's taxonomy level: 'remember', 'understand', 'apply', 'analyze', 'evaluate', 'create'"
    )]
    topic: Annotated[str, Field(
        description="The specific topic this question tests"
    )]
    choices: Annotated[List[AnswerChoice], Field(
        description="List of answer choices (typically 4 for MCQ, 2 for true/false)"
    )]
    correct_answer: Annotated[str, Field(
        description="The label of the correct answer choice, e.g. 'A', 'B', 'C', 'D'"
    )]
    explanation: Annotated[str, Field(
        description="Detailed explanation of why the correct answer is right"
    )]
    hints: Annotated[List[Hint], Field(
        description="Progressive hints from subtle to strong (typically 2-3 hints per question)"
    )]


class QuestionGeneratorOutput(BaseModel):
    """Complete output of the QuestionGenerator agent."""
    stage_id: Annotated[str, Field(
        description="The stage ID this quiz covers, e.g. 'stage_1'"
    )]
    topic: Annotated[str, Field(
        description="The main topic this quiz assesses"
    )]
    difficulty_level: Annotated[str, Field(
        description="Overall quiz difficulty: 'beginner', 'intermediate', 'advanced'"
    )]
    total_questions: Annotated[int, Field(
        description="Total number of questions in the quiz"
    )]
    time_limit_minutes: Annotated[int, Field(
        description="Suggested time limit for the quiz in minutes"
    )]
    passing_score: Annotated[int, Field(
        description="Minimum percentage score to pass the quiz, e.g. 70"
    )]
    questions: Annotated[List[QuizQuestion], Field(
        description="The list of quiz questions with answers and hints"
    )]
