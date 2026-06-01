# Mentra-AI Roadmap & Adaptation API Documentation

This document provides the complete, tested, and validated specifications for the Roadmap Generation (Mode 1), Stage Resources (Mode 2), and Quiz Adaptation (Mode 3) endpoints.

---

## 1. Roadmap Agent: Curriculum Generation (Mode 1)
Used when generating a brand new roadmap for a user from scratch.

* **URL:** `/api/v1/roadmap/`  
* **Method:** `POST`  
* **Headers:**
  - `Content-Type: application/json`
  - `Accept: application/json`

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "weekly_hours": 10,
  "is_stage_progression": false,
  "current_stage": null,
  "curriculum": null,
  "current_stage_index": null,
  "learner_progress": null
}
```

### Response Body — `201 Created`
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
            "name": "HTML5 Fundamentals",
            "topics": [
              "Semantic HTML",
              "Forms and Validations"
            ],
            "learning_objectives": [
              "Build a functional, accessible form"
            ],
            "estimated_weeks": 2,
            "resources": []
          },
          {
            "id": "stage_2",
            "name": "CSS3 Styling",
            "topics": [
              "Flexbox",
              "Grid Layouts"
            ],
            "learning_objectives": [
              "Create fully responsive custom layouts using CSS Grid and Flexbox"
            ],
            "estimated_weeks": 3,
            "resources": []
          }
        ],
        "dependencies": {
          "stage_2": ["stage_1"]
        }
      },
      "total_weeks": 5,
      "difficulty_level": "beginner",
      "skill_gaps": ["HTML", "CSS"]
    },
    "error": null
  }
}
```

---

## 2. Resource Agent: Stage Resources (Mode 2)
Used when a user advances to a new stage and requests the specific learning resources (Videos, Articles) for each topic in that stage. Programmatically guarantees exactly **1 video and 1 article** of the highest rating per topic.

* **URL:** `/api/v1/roadmap/`  
* **Method:** `POST`  
* **Headers:**
  - `Content-Type: application/json`
  - `Accept: application/json`

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "weekly_hours": 10,
  "is_stage_progression": true,
  "current_stage": {
    "id": "stage_1",
    "name": "HTML5 Fundamentals",
    "topics": [
      "Semantic HTML",
      "Forms and Validations"
    ],
    "learning_objectives": [
      "Build a functional, accessible form"
    ],
    "estimated_weeks": 2
  },
  "curriculum": null,
  "current_stage_index": null,
  "learner_progress": null
}
```

### Response Body — `201 Created`
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
        "name": "HTML5 Fundamentals",
        "topics": [
          "Semantic HTML",
          "Forms and Validations"
        ],
        "learning_objectives": [
          "Build a functional, accessible form"
        ],
        "estimated_weeks": 2
      },
      "stage_index": null,
      "stage_resources": {
        "stage_id": "stage_1",
        "topics_resources": [
          {
            "topic_name": "Semantic HTML",
            "videos": [
              {
                "title": "Explained in 4 minutes: Semantic HTML",
                "url": "https://www.youtube.com/watch?v=YPzFPoqwTmI",
                "type": "video",
                "quality_score": 9.5
              }
            ],
            "articles": [
              {
                "title": "Semantic Tags | HTML Tutorial | CodeWithHarry",
                "url": "https://www.codewithharry.com/tutorial/html-semantic-tags",
                "type": "article",
                "quality_score": 9.0
              }
            ],
            "documentation": []
          },
          {
            "topic_name": "Forms and Validations",
            "videos": [
              {
                "title": "Learn HTML forms in 8 minutes ✍",
                "url": "https://www.youtube.com/watch?v=2O8pkybH6po",
                "type": "video",
                "quality_score": 9.5
              }
            ],
            "articles": [
              {
                "title": "mdbootstrap/HTML-Forms-and-validation-tutorial-for-beginners",
                "url": "https://github.com/mdbootstrap/HTML-Forms-and-validation-tutorial-for-beginners",
                "type": "article",
                "quality_score": 9.0
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

## 3. Adaptation Engine: Quiz Remediation (Mode 3)
Used when the learner fails a stage quiz (score < 50%). The agent analyzes specific question failures, merges them into a smart search topic (applicable across any technology/domain), searches for resources, and patches the stage with exactly **2 videos and 2 articles** of remediation.

* **URL:** `/api/v1/quiz/adaptation_stage`  
* **Method:** `POST`  
* **Headers:**
  - `Content-Type: application/json`
  - `Accept: application/json`

### Request Body
```json
{
  "user_id": "test_user_001",
  "career_track": "Frontend Developer",
  "stage_id": "stage_9",
  "stage_name": "React Hooks & State Management",
  "score": 30,
  "difficulty_level": "intermediate",
  "learning_objectives": [
    "Master advanced React Hooks",
    "Optimize performance with useCallback and useMemo",
    "Manage side effects using useEffect"
  ],
  "failed_questions": [
    {
      "question": "What is the primary purpose of the `useState` hook?",
      "user_answer": "To handle side effects in functional components.",
      "correct_answer": "To add state to functional components."
    },
    {
      "question": "Which hook is used for managing side effects, like data fetching or subscriptions?",
      "user_answer": "useState",
      "correct_answer": "useEffect"
    },
    {
      "question": "How can you share state between deeply nested components without prop drilling?",
      "user_answer": "Using local component state (useState)",
      "correct_answer": "Using the Context API (useContext hook)"
    }
  ]
}
```

### Response Body — `201 Created`
```json
{
  "signal": "201_Created",
  "status": "success",
  "message": "Roadmap adaptation successfully",
  "Additional_Resource": {
    "status": "success",
    "mode": "adaptation",
    "user_id": "test_user_001",
    "career_track": "Frontend Developer",
    "stage_id": "stage_9",
    "stage_name": "React Hooks & State Management",
    "score": 30,
    "adapted": true,
    "data": {
      "curriculum": {
        "stages": [
          {
            "id": "stage_9",
            "name": "React Hooks & State Management",
            "topics": [],
            "learning_objectives": [
              "Master advanced React Hooks",
              "Optimize performance with useCallback and useMemo",
              "Manage side effects using useEffect"
            ],
            "estimated_weeks": 1,
            "resources": [
              {
                "title": "React Hooks 1: useState, useEffect, useContext, useRef & useReducer - DEV Community",
                "url": "https://dev.to/carolinacobo/react-hooks-1-usestate-useeffect-usecontext-useref-usereducer-2be2",
                "source": "article",
                "duration_min": 5,
                "difficulty": "intermediate",
                "topic": "React useState, useEffect, and useContext",
                "why": "Top search result for 'React useState, useEffect, and useContext' intermediates tutorial",
                "resource_type": "remedial"
              },
              {
                "title": "Learn React Hooks: useContext - Simply Explained!",
                "url": "https://www.youtube.com/watch?v=HYKDUF8X3qI",
                "source": "article",
                "duration_min": 5,
                "difficulty": "intermediate",
                "topic": "React useState, useEffect, and useContext",
                "why": "Top search result for 'React useState, useEffect, and useContext' intermediates tutorial",
                "resource_type": "remedial"
              },
              {
                "title": "Learn React Hooks: useState - Simply Explained!",
                "url": "https://www.youtube.com/watch?v=V9i3cGD-mts",
                "source": "youtube",
                "duration_min": 1,
                "difficulty": "intermediate",
                "topic": "React useState, useEffect, and useContext",
                "why": "Beginner YouTube tutorial for 'React useState, useEffect, and useContext'",
                "resource_type": "remedial"
              },
              {
                "title": "Learn React Hooks: useContext - Simply Explained!",
                "url": "https://www.youtube.com/watch?v=HYKDUF8X3qI",
                "source": "youtube",
                "duration_min": 1,
                "difficulty": "intermediate",
                "topic": "React useState, useEffect, and useContext",
                "why": "Beginner YouTube tutorial for 'React useState, useEffect, and useContext'",
                "resource_type": "remedial"
              }
            ],
            "adapted": true,
            "adaptation_summary": "The learner failed to grasp the core concepts of React Hooks, specifically confusing the purposes of `useState` and `useEffect`, and not understanding how `useContext` addresses prop drilling. The provided resources offer targeted explanations and examples for these hooks to reinforce understanding. The learner should review these resources before retrying the stage."
          }
        ],
        "last_adapted_stage": "stage_9"
      },
      "stage_id": "stage_9",
      "stage_name": "React Hooks & State Management",
      "score": 30,
      "struggling_topics": [
        "React useState, useEffect, and useContext"
      ],
      "failed_questions": [
        {
          "question": "What is the primary purpose of the `useState` hook?",
          "correct_answer": "To add state to functional components.",
          "user_answer": "To handle side effects in functional components.",
          "topic_gap": "Misunderstanding the purpose of useState vs useEffect."
        },
        {
          "question": "Which hook is used for managing side effects, like data fetching or subscriptions?",
          "correct_answer": "useEffect",
          "user_answer": "useState",
          "topic_gap": "Confusing useState with useEffect for side effect management."
        },
        {
          "question": "How can you share state between deeply nested components without prop drilling?",
          "correct_answer": "Using the Context API (useContext hook)",
          "user_answer": "Using local component state (useState)",
          "topic_gap": "Not understanding how useContext solves prop drilling compared to useState."
        }
      ],
      "stage_adjustments": [
        {
          "action": "insert_remedial",
          "stage_id": "stage_9",
          "reason": "The learner struggled with fundamental hooks like useState, useEffect, and useContext, indicating a need for targeted remedial content before retrying the stage."
        }
      ],
      "summary": "The learner failed to grasp the core concepts of React Hooks, specifically confusing the purposes of `useState` and `useEffect`, and not understanding how `useContext` addresses prop drilling. The provided resources offer targeted explanations and examples for these hooks to reinforce understanding. The learner should review these resources before retrying the stage.",
      "recommended_next_action": "review_resources",
      "total_weeks": null,
      "difficulty_level": "intermediate",
      "skill_gaps": []
    },
    "error": null
  },
  "time_consume": 14.818306799999846
}
```
