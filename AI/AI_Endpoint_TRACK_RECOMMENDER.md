# API Documentation

---

## 1. Track Recommender Agent

### `POST /api/v1/tracks/recommend`

Analyzes a user's career profile (which may be partially complete) and recommends the 3-5 most suitable tech career tracks with fit scores, reasoning, current skill overlap, recommended skills to learn, and estimated transition timelines.

> **Note:** The backend server handles missing profile fields gracefully. The agent calculates a `profile_completeness` percentage and suggests missing fields that would help improve recommendation accuracy.

---

#### Request Body

| Field | Type | Required | Description |
|-------|------|----------|-------------|
| `user_id` | string | ✅ Yes | Unique user identifier |
| `profile` | object | ✅ Yes | User profile features dictionary (all nested fields are optional) |

**`profile` Object Fields:**

| Field | Type | Required | Description / Accepted Values |
|-------|------|----------|-------------------------------|
| `Age` | string | ❌ Optional | User age range (see accepted values below) |
| `EdLevel` | string | ❌ Optional | Highest education level achieved (see accepted values below) |
| `YearsCode` | number (float/int) | ❌ Optional | Total years of coding experience |
| `WorkExp` | number (float/int) | ❌ Optional | Total years of professional work experience |
| `Employment` | string | ❌ Optional | Current employment status (see accepted values below) |
| `RemoteWork` | string | ❌ Optional | Work environment/mode preference (see accepted values below) |
| `Industry` | string | ❌ Optional | Current or most recent industry (see accepted values below) |
| `OrgSize` | string | ❌ Optional | Organisation size (see accepted values below) |
| `AISelect` | string | ❌ Optional | AI tool usage/frequency (see accepted values below) |
| `current_skills` | array of strings | ❌ Optional | Technologies/skills the user currently knows (must map to canonical vocabulary below) |
| `future_skills` | array of strings | ❌ Optional | Technologies/skills the user wants to learn (must map to canonical vocabulary below) |

---

#### Accepted Values for Profile Fields

Categorical values supplied to the `profile` object MUST match the following accepted values from the model dataset schema exactly:

<details>
<summary><b>Age Ranges (Age)</b></summary>

- `18-24 years old`
- `25-34 years old`
- `35-44 years old`
- `45-54 years old`
- `55-64 years old`
- `65 years or older`
- `Prefer not to say`
</details>

<details>
<summary><b>Education Levels (EdLevel)</b></summary>

- `Primary/elementary school`
- `Secondary school (e.g. American high school, German Realschule or Gymnasium, etc.)`
- `Some college/university study without earning a degree`
- `Associate degree (A.A., A.S., etc.)`
- `Bachelor’s degree (B.A., B.S., B.Eng., etc.)`
- `Master’s degree (M.A., M.S., M.Eng., MBA, etc.)`
- `Professional degree (JD, MD, Ph.D, Ed.D, etc.)`
- `Other (please specify):`
</details>

<details>
<summary><b>Employment Status (Employment)</b></summary>

- `Employed`
- `Independent contractor, freelancer, or self-employed`
- `Student`
- `Not employed`
- `I prefer not to say`
</details>

<details>
<summary><b>Remote Work Preferences (RemoteWork)</b></summary>

- `Remote`
- `Hybrid (some in-person, leans heavy to flexibility)`
- `Hybrid (some remote, leans heavy to in-person)`
- `Your choice (very flexible, you can come in when you want or just as needed)`
- `In-person`
</details>

<details>
<summary><b>Industries (Industry)</b></summary>

- `Software Development`
- `Computer Systems Design and Services`
- `Internet, Telecomm or Information Services`
- `Fintech`
- `Banking/Financial Services`
- `Insurance`
- `Healthcare`
- `Retail and Consumer Services`
- `Manufacturing`
- `Transportation, or Supply Chain`
- `Energy`
- `Government`
- `Higher Education`
- `Media & Advertising Services`
- `Other:`
- `null`
</details>

<details>
<summary><b>Organisation Sizes (OrgSize)</b></summary>

- `Just me - I am a freelancer, sole proprietor, etc.`
- `Less than 20 employees`
- `20 to 99 employees`
- `100 to 499 employees`
- `500 to 999 employees`
- `1,000 to 4,999 employees`
- `5,000 to 9,999 employees`
- `10,000 or more employees`
- `I don’t know`
- `null`
</details>

<details>
<summary><b>AI Tool Usage (AISelect)</b></summary>

- `Yes, I use AI tools daily`
- `Yes, I use AI tools weekly`
- `Yes, I use AI tools monthly or infrequently`
- `No, but I plan to soon`
- `No, and I don't plan to`
- `null`
</details>

<details>
<summary><b>Canonical Skills Vocabulary (current_skills / future_skills)</b></summary>

The following skills are supported. When passing skills in the `current_skills` or `future_skills` array, they must be formatted in lowercase as shown below:

| | | | |
|---|---|---|---|
| `ada` | `amazon redshift` | `amazon web services (aws)` | `angular` |
| `angularjs` | `ansible` | `apt` | `asp.net` |
| `asp.net core` | `assembly` | `astro` | `axum` |
| `bash/shell (all shells)` | `bigquery` | `blazor` | `bun` |
| `c` | `c#` | `c++` | `cargo` |
| `cassandra` | `chocolatey` | `clickhouse` | `cloud firestore` |
| `cloudflare` | `cobol` | `cockroachdb` | `composer` |
| `cosmos db` | `dart` | `databricks sql` | `datadog` |
| `datomic` | `delphi` | `deno` | `digital ocean` |
| `django` | `docker` | `drupal` | `duckdb` |
| `dynamodb` | `elasticsearch` | `elixir` | `erlang` |
| `express` | `f#` | `fastapi` | `fastify` |
| `firebase` | `firebase realtime database` | `flask` | `fortran` |
| `gdscript` | `gleam` | `go` | `google cloud` |
| `gradle` | `groovy` | `h2` | `heroku` |
| `homebrew` | `html/css` | `ibm cloud` | `ibm db2` |
| `influxdb` | `java` | `javascript` | `jquery` |
| `kotlin` | `kubernetes` | `laravel` | `lisp` |
| `lua` | `make` | `mariadb` | `matlab` |
| `maven (build tool)` | `micropython` | `microsoft access` | `microsoft azure` |
| `microsoft sql server` | `mojo` | `mongodb` | `msbuild` |
| `mysql` | `neo4j` | `nestjs` | `netlify` |
| `new relic` | `next.js` | `ninja` | `node.js` |
| `npm` | `nuget` | `nuxt.js` | `ocaml` |
| `oracle` | `pacman` | `perl` | `phoenix` |
| `php` | `pip` | `pnpm` | `pocketbase` |
| `podman` | `poetry` | `postgresql` | `powershell` |
| `prolog` | `prometheus` | `python` | `r` |
| `railway` | `react` | `redis` | `ruby` |
| `ruby on rails` | `rust` | `scala` | `snowflake` |
| `splunk` | `spring boot` | `sql` | `sqlite` |
| `supabase` | `svelte` | `swift` | `symfony` |
| `terraform` | `typescript` | `valkey` | `vba` |
| `vercel` | `visual basic (.net)` | `vite` | `vue.js` |
| `webpack` | `wordpress` | `yandex cloud` | `yarn` |
| `zig` | | | |

</details>

---

#### Canonical Career Tracks (Agent Recommendations)

The agent will map the user's profile to **3 to 5** of the following 18 canonical tech tracks:

1. `Frontend Engineering`
2. `Backend Engineering`
3. `Full-Stack Development`
4. `Mobile Development (iOS / Android / Cross-platform)`
5. `DevOps / Site Reliability Engineering (SRE)`
6. `Cloud Architecture / Cloud Engineering`
7. `Data Engineering`
8. `Data Science / Analytics`
9. `Machine Learning Engineering`
10. `MLOps / AI Infrastructure`
11. `Cybersecurity Engineering`
12. `Embedded Systems / IoT`
13. `Game Development`
14. `Blockchain / Web3 Development`
15. `Platform Engineering`
16. `QA / Test Automation Engineering`
17. `Systems Programming`
18. `AI / LLM Application Development`

---

**Request Example:**

```json
{
  "user_id": "user_123",
  "profile": {
    "Age": "25-34 years old",
    "EdLevel": "Bachelor’s degree (B.A., B.S., B.Eng., etc.)",
    "YearsCode": 4.5,
    "WorkExp": 3.0,
    "Employment": "Employed",
    "RemoteWork": "Remote",
    "Industry": "Software Development",
    "OrgSize": "100 to 499 employees",
    "AISelect": "Yes, I use AI tools daily",
    "current_skills": ["python", "fastapi", "sql", "docker"],
    "future_skills": ["rust", "kubernetes", "go"]
  }
}
```

---

#### Response — `201 Created`

| Field | Type | Description |
|-------|------|-------------|
| `signal` | string | `"201_Created"` |
| `status` | string | `"success"` |
| `message` | string | `"Track recommendations generated successfully"` |
| `recommendations` | object | API response payload from the recommendation graph |
| `time_consumed` | float | Execution duration in seconds |

**`recommendations` Payload Structure (Graph `api_response` State):**

| Field | Type | Description |
|-------|------|-------------|
| `status` | string | `"success"` or `"error"` |
| `mode` | string | `"track_recommendation"` |
| `user_id` | string | Unique user identifier |
| `data` | object | Enclosing object for recommendation results |
| `error` | string / null | Error message if graph execution failed |

**`data` Object Structure:**

| Field | Type | Description |
|-------|------|-------------|
| `recommendations` | object | Holds the formatted recommendations details (see below) |

**`data.recommendations` Object Structure (`TrackRecommenderOutput`):**

| Field | Type | Description |
|-------|------|-------------|
| `user_summary` | string | 2-3 sentence overview of user strengths, experience, and key interest signals |
| `recommended_tracks` | array of objects | Top 3 to 5 career tracks sorted by `fit_score` descending (details below) |
| `primary_recommendation` | string | The single absolute best-fit track with a one-sentence justification |
| `profile_completeness` | integer (0-100) | Calculated completeness percentage of the provided profile fields |
| `missing_info_suggestions` | array of strings / null | Input fields that were omitted, but would help refine recommendations if provided |

**Each `recommended_tracks` item (`TrackMatch`):**

| Field | Type | Description |
|-------|------|-------------|
| `track_name` | string | One of the 18 canonical tracks (e.g., `"Backend Engineering"`) |
| `fit_score` | integer (0-100) | Alignment score based on skills overlap and experience signals |
| `reasoning` | string | 2-3 sentences explaining why this track is a fit for the user |
| `skill_overlap` | array of strings | User's current skills that directly apply to this track (normalized to lowercase) |
| `skills_to_learn` | array of strings | Top 3-5 most critical skills the user needs to acquire |
| `estimated_transition_weeks` | integer | Estimated weeks to become job-ready, factoring in experience and skill gaps |

---

**Full Response Example:**

```json
{
  "signal": "201_Created",
  "status": "success",
  "message": "Track recommendations generated successfully",
  "time_consumed": 1.84,
  "recommendations": {
    "status": "success",
    "mode": "track_recommendation",
    "user_id": "user_123",
    "data": {
      "recommendations": {
        "user_summary": "The user is a mid-level professional with 4.5 years of coding experience and a strong background in Python, FastAPI, and Docker. They are highly active with AI tools daily and show deep interest in learning modern systems and infrastructure technologies.",
        "recommended_tracks": [
          {
            "track_name": "Backend Engineering",
            "fit_score": 95,
            "reasoning": "You already have strong foundational skills in Python, FastAPI, and SQL, which are core backend tech. Transitioning fully will focus on mastering high-performance databases, caching, and advanced system design.",
            "skill_overlap": ["python", "fastapi", "sql", "docker"],
            "skills_to_learn": ["postgresql", "redis", "go", "nestjs"],
            "estimated_transition_weeks": 6
          },
          {
            "track_name": "DevOps / Site Reliability Engineering (SRE)",
            "fit_score": 82,
            "reasoning": "Your familiarity with Docker and python scripting provides a great foundation. By learning container orchestration and infrastructure management, you can easily shift towards SRE.",
            "skill_overlap": ["docker", "python"],
            "skills_to_learn": ["kubernetes", "terraform", "ansible", "prometheus"],
            "estimated_transition_weeks": 12
          },
          {
            "track_name": "AI / LLM Application Development",
            "fit_score": 80,
            "reasoning": "Using AI daily combined with a solid Python backend baseline sets you up nicely to construct modern agentic AI apps. Focus on AI orchestrations and framework integrations.",
            "skill_overlap": ["python", "fastapi"],
            "skills_to_learn": ["langchain", "llamaindex", "chromadb", "openai api"],
            "estimated_transition_weeks": 8
          }
        ],
        "primary_recommendation": "Backend Engineering: A near-perfect fit with 95% score, leveraging your strong existing Python backend stack with minimal friction.",
        "profile_completeness": 100,
        "missing_info_suggestions": []
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
| `400` | `400_Bad_Request` | Request body is not valid JSON, or is missing the required `user_id` or `profile` object. |
| `500` | `500_Internal_Server_Error` | Graph initialization failure or LLM runtime exception during evaluation. |

---

### Recommendation Logic & Rules

Recommendations are calculated dynamically using the following guidelines:

1. **Fit Score Evaluation:**
   - **90-100**: Near-perfect alignment (user already has most core skills).
   - **70-89**: Strong fit (meaningful skill overlap + clear interest signals).
   - **50-69**: Moderate fit (transferable skills, but significant upskilling/ramp-up needed).
   - **30-49**: Stretch (possible but requires substantial training/effort).
   - **0-29**: Poor fit (minimal alignment, omitted from recommendations).

2. **Transition Time Estimate:**
   - Experienced developers switching to a related track: **4-12 weeks**.
   - Moderate gaps or career adjustments: **12-24 weeks**.
   - Large skill gaps / junior / new learner: **24-52 weeks**.

3. **Interest Signals (Future Skills):**
   - If the user lists technologies they *want* to learn in `future_skills`, the agent treats these as strong career interests, boosting matching tracks' score even if current skill overlap is minimal.

4. **Profile Completeness Calculations:**
   - Measures how many of the 11 optional profile fields were provided: `(fields_provided / 11) * 100` rounded to the nearest integer.
   - Suggests omitted fields in `missing_info_suggestions` when the completeness is low.
