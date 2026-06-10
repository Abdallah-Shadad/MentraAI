# Mentra-AI Roadmap API Documentation

This document provides the complete specifications for the `roadmap` endpoint, including the 3 different modes of operation.

---

## Endpoint Details
**URL:** `/api/v1/roadmap/`  
**Method:** `POST`  
**Headers:**
- `Content-Type: application/json`
- `Accept: application/json`

---

## Mode 1: Initial Build (Roadmap Overview)
Used when generating a brand new roadmap for a user from scratch.

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "weekly_hours": 15,
  "is_stage_progression": false
}
```

### Response Body
```json
{
  "signal": "201_Created",
  "message": "Roadmap generated successfully",
  "roadmap": {
    "status": "success",
    "mode": "roadmap_overview",
    "user_id": "test_user_001",
    "career_track": "Frontend Developer",
    "data": {
      "curriculum": {
        "stages": [
          {
            "id": "stage_1",
            "name": "HTML Fundamentals",
            "topics": ["Introduction to HTML", "Basic HTML structure", "..."],
            "learning_objectives": {
              "Objective 1": "Proof 1"
            },
            "estimated_weeks": 2
          }
        ],
        "dependencies": {
          "stage_2": ["stage_1"]
        }
      },
      "total_weeks": 48,
      "difficulty_level": "beginner",
      "skill_gaps": ["HTML", "CSS", "JavaScript"]
    },
    "error": null
  }
}
```

---

## Mode 2: Stage Progression (Resource Fetching)
Used when a user advances to a new stage and needs the specific learning resources (Videos, Articles, Documentation) for the topics in that stage.

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "weekly_hours": 15,
  "is_stage_progression": true,
  "current_stage": {
    "id": "stage_1",
    "name": "HTML Fundamentals",
    "topics": [
      "Semantic HTML",
      "HTML forms"
    ],
    "estimated_weeks": 1
  }
}
```

### Response Body
```json
{
  "signal": "201_Created",
  "message": "Roadmap generated successfully",
  "roadmap": {
    "status": "success",
    "mode": "stage_resources",
    "user_id": "test_user_001",
    "career_track": "Frontend Developer",
    "data": {
      "curriculum": {
        "stages": []
      },
      "current_stage": {
        "id": "stage_1",
        "name": "HTML Fundamentals",
        "topics": ["Semantic HTML", "HTML forms"],
        "estimated_weeks": 1
      },
      "stage_index": null,
      "stage_resources": {
        "stage_id": "stage_1",
        "topics_resources": [
          {
            "topic_name": "Semantic HTML",
            "videos": [
              {
                "title": "HTML5 Semantic Elements Tutorial",
                "url": "https://www.youtube.com/watch?v=...",
                "type": "video",
                "quality_score": 9
              }
            ],
            "articles": [
              {
                "title": "A Detailed Guide on HTML Semantics",
                "url": "https://...",
                "type": "article",
                "quality_score": 9
              }
            ],
            "documentation": []
          }
        ]
      },
      "total_weeks": null,
      "difficulty_level": null,
      "skill_gaps": []
    },
    "error": null
  }
}
```

---

## Mode 3: Adaptation Mode (Remediation)
Used when the user fails a quiz (scores < 50%). The system pauses the normal flow and provides targeted remedial resources.

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "weekly_hours": 15,
  "is_stage_progression": true,
  "current_stage": {
    "id": "stage_3",
    "name": "JavaScript Fundamentals",
    "topics": ["Functions and scope", "Variables and data types"],
    "estimated_weeks": 2
  },
  "learner_progress": {
    "stage_id": "stage_3",
    "topic": "JavaScript Fundamentals",
    "score": 40,
    "struggling_topics": ["Functions and scope"],
    "failed_questions": [
      {
        "question": "What is the difference between let and var?",
        "user_answer": "There is no difference.",
        "correct_answer": "let has block scope, while var has function scope."
      }
    ]
  }
}
```

### Response Body
```json
{
  "signal": "201_Created",
  "message": "Roadmap generated successfully",
  "roadmap": {
    "status": "success",
    "mode": "adaptation",
    "user_id": "test_user_001",
    "career_track": "Frontend Developer",
    "data": {
      "curriculum": {
        "stages": [
          {
            "id": "stage_3",
            "name": "JavaScript Fundamentals",
            "resources": [
              {
                "title": "Learn JavaScript VARIABLE SCOPE",
                "url": "https://youtube.com/...",
                "type": "video",
                "quality_score": 9.5,
                "resource_type": "remedial"
              }
            ],
            "adapted": true,
            "adaptation_summary": "We noticed you're struggling with variable scope. Please review these remedial videos."
          }
        ],
        "last_adapted_stage": "stage_3"
      },
      "stage_id": "stage_3",
      "score": 40,
      "struggling_topics": ["Functions and scope"],
      "failed_questions": [
        {
          "question": "What is the difference between let and var?",
          "user_answer": "There is no difference.",
          "correct_answer": "let has block scope, while var has function scope."
        }
      ],
      "stage_adjustments": [
        "Added 1 extra week to review scope and closures."
      ],
      "summary": "We noticed you're struggling with variable scope. Please review these remedial videos.",
      "recommended_next_action": "review_remedial_materials"
    },
    "error": null
  }
}
```
