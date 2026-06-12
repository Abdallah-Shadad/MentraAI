"use client";

import { useMemo, useState } from "react";
import { useRouter } from "@/lib/i18n/navigation";
//components
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import ErrorState from "@/components/reusable/ErrorState";
import SuccessState from "@/components/reusable/SuccessState";
//hooks
import { useOnboarding } from "@/hooks/useOnboarding";

export default function Onboarding() {
  const {
    mutate: submitOnboarding,
    isPending,
    isSuccess,
    isError,
    error,
    reset,
  } = useOnboarding();
  const questions = onboardingData;
  const router = useRouter();
  //step management
  const [step, setStep] = useState(0);
  const next = () => setStep((s) => Math.min(s + 1, 3));
  const back = () => setStep((s) => Math.max(s - 1, 0));

  //answers management
  const [answers, setAnswers] = useState([
    {
      questionId: 1,
      answerText: "",
    },
    {
      questionId: 2,
      answerText: "",
    },
    {
      questionId: 3,
      answerText: 0,
    },
    {
      questionId: 4,
      answerText: 0,
    },
    {
      questionId: 5,
      answerText: "",
    },
    {
      questionId: 6,
      answerText: "",
    },
    {
      questionId: 7,
      answerText: "",
    },
    {
      questionId: 8,
      answerText: "",
    },
    {
      questionId: 9,
      answerText: "",
    },
    {
      questionId: 10,
      answerText: [],
    },
    {
      questionId: 11,
      answerText: [],
    },
  ]);

  //answer setter
  const setAnswer = (id, value) => {
    setAnswers((prev) =>
      prev.map((answer) =>
        answer.questionId === id ? { ...answer, answerText: value } : answer,
      ),
    );
  };

  //submit handler
  const handleSubmit = async () => {
    const formattedAnswers = answers.map((answer) => {
      let formattedVal = answer.answerText;
      if (Array.isArray(formattedVal)) {
        formattedVal = JSON.stringify(formattedVal);
      } else if (typeof formattedVal === "number") {
        formattedVal = String(formattedVal);
      }
      return {
        questionId: answer.questionId,
        answerText: formattedVal,
      };
    });
    submitOnboarding({ answers: formattedAnswers });
  };

  const getValidationErrorMessage = () => {
    const errorData = error?.response?.data?.error;
    if (!errorData) return "An error occurred";
    if (errorData.errors) {
      const messages = Object.values(errorData.errors).flat();
      if (messages.length > 0) {
        return messages.join(" | ");
      }
    }
    return errorData.message || "Validation failed";
  };

  return (
    <div className="min-h-screen bg-background text-foreground flex flex-col">
      {isSuccess && (
        <SuccessState
          close={() => {
            reset();
            router.push("/student");
          }}
        />
      )}
      {isError && (
        <ErrorState
          message={getValidationErrorMessage()}
          close={() => {
            reset();
          }}
        />
      )}
      <div className="flex-1 flex items-start justify-center">
        {/* Step 0: Personal Info */}
        {step === 0 && (
          <div className="w-full space-y-6">
            <ChoiceGroup
              question={questions[0]}
              value={
                answers.find((a) => a.questionId === questions[0].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(1, v)}
              layout="flex"
            />

            <ChoiceGroup
              question={questions[1]}
              value={
                answers.find((a) => a.questionId === questions[1].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(2, v)}
            />

            <NumberField
              label={questions[2].questionText}
              value={
                answers.find((a) => a.questionId === questions[2].questionId)
                  ?.answerText || 0
              }
              onChange={(v) => setAnswer(3, v)}
            />

            <NumberField
              label={questions[3].questionText}
              value={
                answers.find((a) => a.questionId === questions[3].questionId)
                  ?.answerText || 0
              }
              onChange={(v) => setAnswer(4, v)}
            />
          </div>
        )}

        {/* Step 1: Career Profile */}
        {step === 1 && (
          <StepContainer title="Career Profile">
            <ChoiceGroup
              question={questions[4]}
              value={
                answers.find((a) => a.questionId === questions[4].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(5, v)}
              layout="flex"
            />

            <ChoiceGroup
              question={questions[5]}
              value={
                answers.find((a) => a.questionId === questions[5].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(6, v)}
            />

            <ChoiceGroup
              question={questions[6]}
              value={
                answers.find((a) => a.questionId === questions[6].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(7, v)}
            />

            <ChoiceGroup
              question={questions[7]}
              value={
                answers.find((a) => a.questionId === questions[7].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(8, v)}
            />
          </StepContainer>
        )}

        {/* Step 2: AI & Tech Usage */}
        {step === 2 && (
          <StepContainer title="AI & Tech Usage">
            <ChoiceGroup
              question={questions[8]}
              value={
                answers.find((a) => a.questionId === questions[8].questionId)
                  ?.answerText
              }
              onChange={(v) => setAnswer(9, v)}
            />

            <MultiSelect
              question={questions[9]}
              value={
                answers.find((a) => a.questionId === questions[9].questionId)
                  ?.answerText || []
              }
              onChange={(v) => setAnswer(10, v)}
            />
          </StepContainer>
        )}

        {step === 3 && (
          <StepContainer title="Learning Goals">
            <MultiSelect
              question={questions[10]}
              value={
                answers.find((a) => a.questionId === questions[10].questionId)
                  ?.answerText || []
              }
              onChange={(v) => setAnswer(11, v)}
            />
          </StepContainer>
        )}
      </div>

      <div className="p-6 flex justify-between border-t border-border mt-6">
        <Button
          variant="outline"
          onClick={back}
          disabled={step === 0}
          className="hover:bg-surface/80"
        >
          Back
        </Button>

        <Button
          className={`bg-primary ${step === 3 && "gradient-cta px-12"} text-white hover:bg-primary/85 cursor-pointer`}
          onClick={step === 3 ? handleSubmit : next}
          disabled={isPending}
        >
          {isPending ? "Submitting..." : step === 3 ? "Submit" : "Next"}
        </Button>
      </div>
    </div>
  );
}

/* ===========================
   STEP CONTAINER
=========================== */

function StepContainer({ title, children }) {
  return (
    <div className="w-full max-w-2xl space-y-6">
      <h2 className="text-2xl font-semibold">{title}</h2>
      <div className="space-y-4">{children}</div>
    </div>
  );
}

/* ===========================
   CHOICE GROUP
=========================== */

function ChoiceGroup({ question, value, onChange, layout = "grid" }) {
  return (
    <div className="space-y-2">
      <p className="text-foreground font-medium text-base md:text-lg lg:text-xl mb-4">
        {question.questionText}
      </p>
      <div
        className={
          layout === "grid" ? "grid grid-cols-2 gap-3" : "flex flex-wrap gap-3"
        }
      >
        {question.options.map((opt) => (
          <div
            key={opt}
            onClick={() => onChange(opt)}
            className={`p-3 rounded-lg border cursor-pointer transition flex items-center gap-2
          ${
            value === opt
              ? "border-primary bg-[rgba(139,92,246,0.1)]"
              : "border-border hover:border-primary"
          }`}
          >
            <div className="text-foreground w-4 h-4 rounded-full border border-border flex items-center justify-center mr-2 shrink-0">
              {value === opt && (
                <span className="w-2 h-2 rounded-full bg-primary"></span>
              )}
            </div>

            <p className="text-foreground font-medium text-sm md:text-base">
              {opt}
            </p>
          </div>
        ))}
      </div>
    </div>
  );
}

/* ===========================
   NUMBER FIELD
=========================== */

function NumberField({ label, value, onChange }) {
  return (
    <div className="space-y-2">
      <p className="text-foreground font-medium text-base md:text-lg lg:text-xl mb-4">
        {label}
      </p>
      <input
        type="number"
        value={value}
        min="0"
        onChange={(e) => onChange(Number(e.target.value))}
        className="w-full p-3 rounded-lg card border border-border"
      />
    </div>
  );
}

/* ===========================
   MULTI SELECT (SKILLS)
=========================== */

function MultiSelect({ question, value, onChange }) {
  const toggle = (item) => {
    if (value.includes(item)) {
      onChange(value.filter((v) => v !== item));
    } else {
      onChange([...value, item]);
    }
  };

  return (
    <div className="space-y-2">
      <p className="text-foreground font-medium text-base md:text-lg lg:text-xl mb-4">
        {question.questionText}
      </p>

      <div className="flex flex-wrap gap-2 max-h-[220px] overflow-auto p-4 border border-border rounded-lg">
        {question.options.map((opt) => (
          <Badge
            key={opt}
            onClick={() => toggle(opt)}
            className={`cursor-pointer ${
              value.includes(opt)
                ? "bg-primary text-muted dark:text-foreground"
                : "bg-card"
            } px-4 py-2`}
          >
            {opt}
          </Badge>
        ))}
      </div>
    </div>
  );
}

/* ===========================
   DATA (from your JSON)
=========================== */

const onboardingData = [
  {
    questionId: 1,
    questionKey: "Age",
    questionText: "What is your age range?",
    questionType: "Choice",
    options: [
      "18-24 years old",
      "25-34 years old",
      "35-44 years old",
      "45-54 years old",
      "55-64 years old",
      "65 years or older",
      "Prefer not to say",
    ],
    displayOrder: 1,
  },
  {
    questionId: 2,
    questionKey: "EdLevel",
    questionText: "What is your highest education level achieved?",
    questionType: "Choice",
    options: [
      "Primary/elementary school",
      "Secondary school (e.g. American high school, German Realschule or Gymnasium, etc.)",
      "Some college/university study without earning a degree",
      "Associate degree (A.A., A.S., etc.)",
      "Bachelor's degree (B.A., B.S., B.Eng., etc.)",
      "Master's degree (M.A., M.S., M.Eng., MBA, etc.)",
      "Professional degree (JD, MD, Ph.D, Ed.D, etc.)",
      "Other (please specify):",
    ],
    displayOrder: 2,
  },
  {
    questionId: 3,
    questionKey: "YearsCode",
    questionText: "How many years of coding experience do you have?",
    questionType: "Number",
    options: [],
    displayOrder: 3,
  },
  {
    questionId: 4,
    questionKey: "WorkExp",
    questionText: "How many years of professional work experience do you have?",
    questionType: "Number",
    options: [],
    displayOrder: 4,
  },
  {
    questionId: 5,
    questionKey: "Employment",
    questionText: "What is your current employment status?",
    questionType: "Choice",
    options: [
      "Employed",
      "Student",
      "Not employed",
      "Independent contractor, freelancer, or self-employed",
    ],
    displayOrder: 5,
  },
  {
    questionId: 6,
    questionKey: "RemoteWork",
    questionText: "What is your work environment preference?",
    questionType: "Choice",
    options: [
      "Remote",
      "Hybrid (some in-person, leans heavy to flexibility)",
      "Hybrid (some remote, leans heavy to in-person)",
      "Your choice (very flexible, you can come in when you want or just as needed)",
      "In-person",
    ],
    displayOrder: 6,
  },
  {
    questionId: 7,
    questionKey: "Industry",
    questionText: "What is your current or most recent industry?",
    questionType: "Choice",
    options: [
      "Software Development",
      "Computer Systems Design and Services",
      "Internet, Telecomm or Information Services",
      "Fintech",
      "Banking/Financial Services",
      "Insurance",
      "Healthcare",
      "Retail and Consumer Services",
      "Manufacturing",
      "Transportation, or Supply Chain",
      "Energy",
      "Government",
      "Higher Education",
      "Media & Advertising Services",
      "Other:",
      "null",
    ],
    displayOrder: 7,
  },
  {
    questionId: 8,
    questionKey: "OrgSize",
    questionText: "What is the size of your organisation?",
    questionType: "Choice",
    options: [
      "Just me - I am a freelancer, sole proprietor, etc.",
      "Less than 20 employees",
      "20 to 99 employees",
      "100 to 499 employees",
      "500 to 999 employees",
      "1,000 to 4,999 employees",
      "5,000 to 9,999 employees",
      "10,000 or more employees",
      "I don't know",
      "null",
    ],
    displayOrder: 8,
  },
  {
    questionId: 9,
    questionKey: "AISelect",
    questionText: "How often do you use AI tools?",
    questionType: "Choice",
    options: [
      "Yes, I use AI tools daily",
      "Yes, I use AI tools weekly",
      "Yes, I use AI tools monthly or infrequently",
      "No, but I plan to soon",
      "No, and I don't plan to",
      "null",
    ],
    displayOrder: 9,
  },
  {
    questionId: 10,
    questionKey: "current_skills",
    questionText: "What technologies and skills do you currently know?",
    questionType: "MultiSelect",
    options: [
      "ada",
      "amazon redshift",
      "amazon web services (aws)",
      "angular",
      "angularjs",
      "ansible",
      "apt",
      "asp.net",
      "asp.net core",
      "assembly",
      "astro",
      "axum",
      "bash/shell (all shells)",
      "bigquery",
      "blazor",
      "bun",
      "c",
      "c#",
      "c++",
      "cargo",
      "cassandra",
      "chocolatey",
      "clickhouse",
      "cloud firestore",
      "cloudflare",
      "cobol",
      "cockroachdb",
      "composer",
      "cosmos db",
      "dart",
      "databricks sql",
      "datadog",
      "datomic",
      "delphi",
      "deno",
      "digital ocean",
      "django",
      "dynamodb",
      "express",
      "firebase",
      "gdscript",
      "gradle",
      "homebrew",
      "influxdb",
      "kotlin",
      "lua",
      "maven (build tool)",
      "microsoft sql server",
      "mysql",
      "new relic",
      "npm",
      "oracle",
      "php",
      "podman",
      "prolog",
      "railway",
      "ruby on rails",
      "splunk",
      "supabase",
      "terraform",
      "vercel",
      "docker",
      "elasticsearch",
      "f#",
      "firebase realtime database",
      "gleam",
      "groovy",
      "html/css",
      "java",
      "kubernetes",
      "make",
      "micropython",
      "drupal",
      "elixir",
      "fastapi",
      "flask",
      "duckdb",
      "erlang",
      "fastify",
      "fortran",
      "mojo",
      "go",
      "h2",
      "ibm cloud",
      "javascript",
      "laravel",
      "mariadb",
      "microsoft access",
      "mongodb",
      "webpack",
      "zig",
      "google cloud",
      "heroku",
      "ibm db2",
      "jquery",
      "lisp",
      "matlab",
      "microsoft azure",
      "msbuild",
      "neo4j",
      "next.js",
      "nuget",
      "pacman",
      "pip",
      "poetry",
      "prometheus",
      "react",
      "rust",
      "spring boot",
      "svelte",
      "typescript",
      "visual basic (.net)",
      "wordpress",
      "nestjs",
      "ninja",
      "nuxt.js",
      "perl",
      "pnpm",
      "postgresql",
      "python",
      "redis",
      "scala",
      "sql",
      "swift",
      "valkey",
      "vite",
      "netlify",
      "node.js",
      "ocaml",
      "phoenix",
      "pocketbase",
      "powershell",
      "r",
      "ruby",
      "snowflake",
      "sqlite",
      "symfony",
      "vba",
      "vue.js",
      "yandex cloud",
      "yarn",
    ],
    displayOrder: 10,
  },
  {
    questionId: 11,
    questionKey: "future_skills",
    questionText: "What technologies and skills do you want to learn?",
    questionType: "MultiSelect",
    options: [
      "ada",
      "amazon redshift",
      "amazon web services (aws)",
      "angular",
      "angularjs",
      "ansible",
      "apt",
      "asp.net",
      "asp.net core",
      "assembly",
      "astro",
      "axum",
      "bash/shell (all shells)",
      "bigquery",
      "blazor",
      "bun",
      "c",
      "c#",
      "c++",
      "cargo",
      "cassandra",
      "chocolatey",
      "clickhouse",
      "cloud firestore",
      "cloudflare",
      "cobol",
      "cockroachdb",
      "composer",
      "cosmos db",
      "dart",
      "databricks sql",
      "datadog",
      "datomic",
      "delphi",
      "deno",
      "digital ocean",
      "django",
      "dynamodb",
      "express",
      "firebase",
      "gdscript",
      "gradle",
      "homebrew",
      "influxdb",
      "kotlin",
      "lua",
      "maven (build tool)",
      "microsoft sql server",
      "mysql",
      "new relic",
      "npm",
      "oracle",
      "php",
      "podman",
      "prolog",
      "railway",
      "ruby on rails",
      "splunk",
      "supabase",
      "terraform",
      "vercel",
      "docker",
      "elasticsearch",
      "f#",
      "firebase realtime database",
      "gleam",
      "groovy",
      "html/css",
      "java",
      "kubernetes",
      "make",
      "micropython",
      "drupal",
      "elixir",
      "fastapi",
      "flask",
      "duckdb",
      "erlang",
      "fastify",
      "fortran",
      "mojo",
      "go",
      "h2",
      "ibm cloud",
      "javascript",
      "laravel",
      "mariadb",
      "microsoft access",
      "mongodb",
      "webpack",
      "zig",
      "google cloud",
      "heroku",
      "ibm db2",
      "jquery",
      "lisp",
      "matlab",
      "microsoft azure",
      "msbuild",
      "neo4j",
      "next.js",
      "nuget",
      "pacman",
      "pip",
      "poetry",
      "prometheus",
      "react",
      "rust",
      "spring boot",
      "svelte",
      "typescript",
      "visual basic (.net)",
      "wordpress",
      "nestjs",
      "ninja",
      "nuxt.js",
      "perl",
      "pnpm",
      "postgresql",
      "python",
      "redis",
      "scala",
      "sql",
      "swift",
      "valkey",
      "vite",
      "netlify",
      "node.js",
      "ocaml",
      "phoenix",
      "pocketbase",
      "powershell",
      "r",
      "ruby",
      "snowflake",
      "sqlite",
      "symfony",
      "vba",
      "vue.js",
      "yandex cloud",
      "yarn",
    ],
    displayOrder: 11,
  },
];
