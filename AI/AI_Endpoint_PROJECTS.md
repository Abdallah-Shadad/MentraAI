# API Documentation

---

## 1. Projects Agent

### `POST /api/v1/projects/recommend`

Generates job-market-aligned project recommendations for a learner based on their current curriculum stage and progress. Recommendations can be tailored specifically to consolidate a single stage, or span across multiple stages as a progressive capstone project.

> **Note:** The backend server is responsible for displaying recommended projects to the learner after completing a stage, tracking milestones progression, and rendering portfolio tips for resume/career guidance.

---

#### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user_id` | string | ✅ Yes | Unique user identifier |
| `career_track` | string | ✅ Yes | Target career track, e.g. `"Web Development"` or `"Data Science"` |
| `stage_id` | string | ✅ Yes | Unique identifier of the curriculum stage, e.g. `"stage_1"` |
| `topics` | array of strings | ✅ Yes | Topics covered in the current stage, e.g. `["HTML5 Tags", "Semantic HTML"]` |
| `stage_name` | string | ❌ Optional | Human-readable name of the stage (defaults to `stage_id`) |
| `difficulty_level` | string | ❌ Optional | `"beginner"` / `"intermediate"` / `"advanced"` (defaults to `"beginner"`) |
| `learning_objectives` | object | ❌ Optional | Key-value mapping of objectives, e.g. `{"Build a form": "Create an accessible HTML form"}` |
| `completed_stages` | array of objects | ❌ Optional | List of stages already completed by the user (provides context for multi-stage projects) |
| `upcoming_stages` | array of objects | ❌ Optional | List of upcoming stages in the curriculum (allows planning progressive milestones) |

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
  "difficulty_level": "beginner",
  "completed_stages": [
    {
      "id": "stage_0",
      "name": "Intro to Web",
      "topics": ["Internet Basics", "HTTP Protocols"]
    }
  ],
  "upcoming_stages": [
    {
      "id": "stage_2",
      "name": "CSS3 Basics",
      "topics": ["Selectors", "Flexbox", "Grid"]
    }
  ]
}
```

---

#### Response — `201 Created`

| Field | Type | Description |
|-------|------|-------------|
| `signal` | string | `"201_Created"` |
| `status` | string | `"success"` |
| `message` | string | Confirmation message |
| `projects` | object | Core response payload containing recommendations (see below) |
| `time_consumed` | float | Processing time in seconds |

**`projects` payload structure (State api_response):**

| Field | Type | Description |
|-------|------|-------------|
| `status` | string | `"success"` or `"error"` |
| `mode` | string | `"project_recommendations"` |
| `user_id` | string | Unique user identifier |
| `career_track` | string | Tailored career track |
| `stage_id` | string | Target stage ID |
| `data` | object | Holds the actual recommendations (see `data` object structure) |
| `error` | string / null | Error message if graph failed, otherwise `null` |

**`data` object:**

| Field | Type | Description |
|-------|------|-------------|
| `recommendations` | object | The curated recommendation details |

**`recommendations` object:**

| Field | Type | Description |
|-------|------|-------------|
| `career_track` | string | Tailored career track |
| `current_stage_id` | string | Current curriculum stage ID |
| `recommendation_mode` | string | `"after_stage"` (single-stage focused) / `"multi_stage"` (spans multiple stages) / `"both"` |
| `projects` | array | List of recommended project objects (1–3 projects) |
| `summary` | string | Motivational summary explaining why these projects are chosen and how they align with market needs |

**Each project recommendation object:**

| Field | Type | Description |
|-------|------|-------------|
| `project_id` | string | Unique identifier, e.g. `"proj_1"` |
| `title` | string | Project title |
| `description` | string | 2–3 sentence overview and market rationale |
| `project_type` | string | `"portfolio_piece"` / `"capstone"` / `"mini_project"` / `"open_source_contribution"` |
| `difficulty` | string | `"beginner"` / `"intermediate"` / `"advanced"` |
| `technologies` | array of strings | Tools/technologies utilized, e.g. `["HTML5", "CSS3", "GitHub Pages"]` |
| `market_relevance` | string | Detailed explanation of job market relevance, targeted roles, and hiring demand |
| `covers_stages` | array of strings | Curriculum stage IDs this project spans |
| `milestones` | array | Ordered milestones mapping to specific curriculum stages (see below) |
| `estimated_total_hours` | integer | Combined estimated duration across all milestones |
| `portfolio_tips` | array of strings | Actionable advice for hosting, repository setup, and showcasing on a resume |

**Each milestone object:**

| Field | Type | Description |
|-------|------|-------------|
| `milestone_id` | string | Unique milestone identifier, e.g. `"ms_1"` |
| `title` | string | Milestone title, e.g. `"Set up HTML Scaffold"` |
| `description` | string | Detailed guide on what components/code the learner needs to build |
| `mapped_stage_id` | string | Aligns this milestone to a stage, e.g. `"stage_1"` |
| `skills_applied` | array of strings | Specific topics/skills from that stage utilized |
| `deliverables` | array of strings | Concrete outputs produced, e.g. `"index.html file with form elements"` |
| `estimated_hours` | integer | Estimated hours to complete |

---

**Full Response Example:**

```json
{
  "signal": "201_Created",
  "status": "success",
  "message": "Project recommendations generated successfully",
  "time_consumed": 2.45,
  "projects": {
    "status": "success",
    "mode": "project_recommendations",
    "user_id": "user_123",
    "career_track": "Web Development",
    "stage_id": "stage_1",
    "data": {
      "recommendations": {
        "career_track": "Web Development",
        "current_stage_id": "stage_1",
        "recommendation_mode": "after_stage",
        "projects": [
          {
            "project_id": "proj_1",
            "title": "Accessible Profile Card and Bio Portal",
            "description": "Build an online profile interface featuring semantically structured cards and detailed bios. This project focuses on utilizing modern semantic tags and forms to establish web accessibility foundations.",
            "project_type": "mini_project",
            "difficulty": "beginner",
            "technologies": ["HTML5", "Semantic HTML", "Forms", "WAI-ARIA"],
            "market_relevance": "Web Accessibility (a11y) is a key differentiator in frontend hiring. Junior Frontend Developers who can build keyboard-navigable and screen-reader-friendly layouts stand out in candidate pools.",
            "covers_stages": ["stage_1"],
            "milestones": [
              {
                "milestone_id": "ms_1",
                "title": "Semantic Layout & Structure",
                "description": "Construct the basic page shell using <header>, <main>, <article>, and <footer> tags. Implement list elements and profile sections to group content logically without relying on unstyled divs.",
                "mapped_stage_id": "stage_1",
                "skills_applied": ["HTML5 Tags", "Semantic HTML"],
                "deliverables": ["index.html containing a semantic layout shell"],
                "estimated_hours": 3
              },
              {
                "milestone_id": "ms_2",
                "title": "Accessible Contact and Inquiry Form",
                "description": "Create a detailed inquiry/contact form inside the profile card using appropriate labels, inputs, placeholder text, and fieldsets. Add accessibility properties like aria-describedby for form validations.",
                "mapped_stage_id": "stage_1",
                "skills_applied": ["Forms", "Semantic HTML"],
                "deliverables": ["Form module with text inputs, dropdowns, and button controls"],
                "estimated_hours": 4
              }
            ],
            "estimated_total_hours": 7,
            "portfolio_tips": [
              "Host the finished page on GitHub Pages.",
              "Validate the HTML structure using the official W3C validator and share a badge/screenshot in the README.",
              "Add a brief section in the README explaining how keyboard navigation and screen readers interpret the semantic tags."
            ]
          }
        ],
        "summary": "These projects are selected to establish a concrete, interactive portfolio piece focused on the fundamental building blocks of modern web engineering: Semantic structure and web accessibility. Having a clean, standard-compliant repo sets a strong base for your subsequent CSS styling stage."
      }
    },
    "error": null
  }
}
```

---

#### Error Responses

| Status | Signal | When |
|--------|--------|------|
| `400` | `400_Bad_Request` | Missing required fields (`user_id`, `career_track`, `stage_id`, `topics`) or invalid JSON request body |
| `500` | `500_Internal_Server_Error` | Graph initialization failure or LLM recommendation runtime exception |

---

### Project Design Guidelines (By Career Track)

Projects are dynamically aligned with current job-market demands using the following criteria:

- **Web Development**: Emphasizes full-stack CRUD setups, OAuth/JWT authentication, REST/GraphQL API development, responsive design & accessibility (WCAG), real-time notifications/WebSockets, CI/CD pipelines, and cloud hosting.
- **Data Science / ML / AI**: Emphasizes end-to-end ML pipelines, dashboard visualizations (Streamlit/Plotly), NLP/RAG architectures, MLOps tooling (experiment tracking, model registries), and prompt engineering.
- **Mobile Development**: Focuses on cross-platform architectures (Flutter, React Native), offline-first capabilities, push notifications, and device API integrations.
- **DevOps / Cloud / SRE**: Emphasizes Infrastructure as Code (Terraform), container orchestration (Kubernetes), observability dashboards (Prometheus/Grafana), and CI/CD automation.
- **Cybersecurity**: Focuses on vulnerability assessment tooling, log analyzers, intrusion alerts automation, and compliance scanning tools.
- **Backend / Systems**: Emphasizes high-throughput APIs (Go/FastAPI), caching layers (Redis), messaging services (Kafka/RabbitMQ), and robust database schema migrations.
