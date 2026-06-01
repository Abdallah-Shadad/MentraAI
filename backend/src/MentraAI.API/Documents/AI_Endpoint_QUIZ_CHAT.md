# API Documentation

---

## 1. Quiz Agent

### `POST /api/v1/quiz/generate`

Generates quiz questions for a specific curriculum stage. Each question includes the correct answer, explanation, and progressive hints.

> **Note:** The backend server is responsible for hiding answers/hints from the frontend, evaluating submitted answers, and serving hints progressively.

---

#### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user_id` | string | ✅ Yes | Unique user identifier |
| `career_track` | string | ✅ Yes | e.g. `"Web Development"` |
| `stage_id` | string | ✅ Yes | e.g. `"stage_1"` |
| `topics` | array of strings | ✅ Yes | List of topics to cover |
| `stage_name` | string | ❌ Optional | e.g. `"HTML5 Fundamentals"` (defaults to `stage_id`) |
| `learning_objectives` | object | ❌ Optional | Key-value map of objectives |
| `difficulty_level` | string | ❌ Optional | `"beginner"` / `"intermediate"` / `"advanced"` (defaults to `"beginner"`) |

**Example:**

```json
{
  "user_id": "user_123",
  "career_track": "Web Development",
  "stage_id": "stage_1",
  "stage_name": "HTML5 Fundamentals",
  "topics": ["HTML5 Tags", "Semantic HTML", "Forms"],
  "learning_objectives": {
    "Build a form": "Create an accessible HTML form"
  },
  "difficulty_level": "beginner"
}
```

---

#### Response — `201 Created`

| Field | Type | Description |
|-------|------|-------------|
| `signal` | string | `"201_Created"` |
| `status` | string | `"success"` |
| `message` | string | Confirmation message |
| `quiz` | object | Full quiz data (see below) |
| `time_consumed` | float | Processing time in seconds |

**`quiz` object:**

| Field | Type | Description |
|-------|------|-------------|
| `stage_id` | string | Stage identifier |
| `topic` | string | Stage name |
| `difficulty_level` | string | Difficulty of the quiz |
| `total_questions` | integer | Number of questions |
| `time_limit_minutes` | integer | Suggested time limit |
| `passing_score` | integer | Minimum passing score (e.g. `70`) |
| `questions` | array | List of question objects |

**Each question object:**

| Field | Type | Description |
|-------|------|-------------|
| `question_id` | string | e.g. `"q_1"` |
| `question_text` | string | The question |
| `question_type` | string | e.g. `"multiple_choice"` |
| `difficulty` | string | `"easy"` / `"medium"` / `"hard"` |
| `bloom_level` | string | e.g. `"understand"`, `"apply"` |
| `topic` | string | Which topic this covers |
| `choices` | array | List of `{ label, text, is_correct }` |
| `correct_answer` | string | Label of correct choice, e.g. `"B"` |
| `explanation` | string | Why the answer is correct |
| `hints` | array | 3 progressive hints (see below) |

**Each hint:**

```json
{ "level": 1, "text": "subtle nudge..." },
{ "level": 2, "text": "moderate guidance..." },
{ "level": 3, "text": "strong clue..." }
```

**Full Response Example:**

```json
{
  "signal": "201_Created",
  "status": "success",
  "message": "Quiz generated successfully",
  "time_consumed": 3.21,
  "quiz": {
    "stage_id": "stage_1",
    "topic": "HTML5 Fundamentals",
    "difficulty_level": "beginner",
    "total_questions": 5,
    "time_limit_minutes": 10,
    "passing_score": 70,
    "questions": [
      {
        "question_id": "q_1",
        "question_text": "Which tag defines a navigation section?",
        "question_type": "multiple_choice",
        "difficulty": "easy",
        "bloom_level": "understand",
        "topic": "Semantic HTML",
        "choices": [
          { "label": "A", "text": "<div>", "is_correct": false },
          { "label": "B", "text": "<nav>", "is_correct": true },
          { "label": "C", "text": "<section>", "is_correct": false },
          { "label": "D", "text": "<header>", "is_correct": false }
        ],
        "correct_answer": "B",
        "explanation": "<nav> is the semantic tag for navigation links.",
        "hints": [
          { "level": 1, "text": "Think about what 'navigation' abbreviates to." },
          { "level": 2, "text": "HTML5 introduced semantic tags for common page regions." },
          { "level": 3, "text": "The tag name is a 3-letter abbreviation of 'navigation'." }
        ]
      }
    ]
  }
}
```

---

#### Error Responses

| Status | Signal | When |
|--------|--------|------|
| `400` | `400_Bad_Request` | Missing required fields or invalid JSON |
| `500` | `500_Internal_Server_Error` | Graph init or generation failure |

---

---

## 2. Chat Agent

### `POST /api/v1/chat/`

Sends a message to the AI mentor. Streams a personalized response based on the learner's current context (career track, stage, lesson, quiz score). Memory is isolated per `conversation_id`.

---

#### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user_id` | string | ✅ Yes | Unique user identifier |
| `conversation_id` | string | ✅ Yes | Scopes the memory — same ID continues the conversation, new ID starts fresh |
| `query` | string | ✅ Yes | The user's message |
| `career_track` | string | ❌ Optional | e.g. `"backend"` |
| `stage` | string | ❌ Optional | e.g. `"advanced_python"` |
| `lesson_id` | string | ❌ Optional | e.g. `"decorators_intro"` |
| `quiz_details` | string | ❌ Optional | e.g. `"quiz_title+quiz_lesson"` |
| `quiz_score` | integer | ❌ Optional | Score from `0` to `100` — if below `70`, mentor focuses on correcting misconceptions |

> All optional fields are safe to omit. If provided, they personalize the AI mentor's system prompt automatically.

**Example:**

```json
{
  "user_id": "user_123",
  "conversation_id": "conv_abc",
  "query": "Can you explain how decorators work in Python?",
  "career_track": "backend",
  "stage": "advanced_python",
  "lesson_id": "decorators_intro",
  "quiz_details": "decorators_quiz+decorators_intro",
  "quiz_score": 55
}
```

---

#### Response — `200 OK` (Streaming)

The response is a **Server-Sent Events (SSE) stream**. Each chunk contains a piece of the AI's reply. A special `X-Conversation-Id` header is included for debugging.

**Response Headers:**

```
X-Conversation-Id: conv_abc
Content-Type: text/event-stream
```

**Stream chunks example:**

```
data: Sure! A decorator in Python is...

data: a function that wraps another function...

data: [DONE]
```

When `quiz_score < 70`, the mentor's system prompt automatically includes a reinforcement instruction like:

```
[LEARNER PROFILE]
Career Track : backend
Current Stage: advanced_python
Active Lesson: decorators_intro
Quiz         : decorators_quiz — Score: 55/100 ⚠️ (needs reinforcement)

You are a personalized AI mentor. Tailor every answer to reinforce the
lesson above. Focus on correcting misconceptions.
```

---

### `DELETE /api/v1/chat/memory/`

Deletes the conversation memory for a specific user and conversation.

#### Request Body

```json
{
  "user_id": "user_123",
  "conversation_id": "conv_abc"
}
```

Deletes only the Redis keys for that specific conversation:

- `chat:memory:{user_id}:{conversation_id}:messages`
- `chat:memory:{user_id}:{conversation_id}:summary`
- `chat:memory:{user_id}:{conversation_id}:msg_count`

#### Response — `200 OK`

```json
{
  "signal": "200_OK",
  "message": "Conversation memory cleared."
}
```

---

### `GET /api/v1/chat/health`

Health check for the Chat Agent service.

#### Response — `200 OK`

```json
{
  "status": "ok"
}
```

---

## Memory & Conversation Behavior

| Scenario | Result |
|----------|--------|
| Same `conversation_id` used twice | Second message remembers the first ✅ |
| New `conversation_id` for same user | Fresh conversation, no previous memory ✅ |
| `quiz_score` below 70 | Mentor focuses on correcting misconceptions ✅ |
| Optional fields omitted | That block is skipped from the prompt — no errors ✅ |
