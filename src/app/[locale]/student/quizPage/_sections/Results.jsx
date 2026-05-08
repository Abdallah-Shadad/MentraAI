"use client";
import { Link } from "@/lib/i18n/navigation";
import { Sparkles, Brain, RefreshCw, ArrowRight } from "lucide-react";
//questions data
export const QUESTIONS = [
  {
    id: 1,
    concept: "Variables & Scope",
    prompt:
      "What value will be logged after running this snippet? `let x = 1; { let x = 2; } console.log(x);`",
    options: [
      {
        id: "a",
        label: "1 — the inner block has its own scope",
      },
      {
        id: "b",
        label: "2 — the inner assignment overrides the outer",
      },
      {
        id: "c",
        label: "undefined — x is shadowed",
      },
      {
        id: "d",
        label: "ReferenceError",
      },
    ],
    correct: "a",
  },
  {
    id: 2,
    concept: "Variables & Scope",
    prompt:
      "What value will be logged after running this snippet? `let x = 1; { let x = 2; } console.log(x);`",
    options: [
      {
        id: "a",
        label: "1 — the inner block has its own scope",
      },
      {
        id: "b",
        label: "2 — the inner assignment overrides the outer",
      },
      {
        id: "c",
        label: "undefined — x is shadowed",
      },
      {
        id: "d",
        label: "ReferenceError",
      },
    ],
    correct: "a",
  },
  {
    id: 3,
    concept: "Variables & Scope",
    prompt:
      "What value will be logged after running this snippet? `let x = 1; { let x = 2; } console.log(x);`",
    options: [
      {
        id: "a",
        label: "1 — the inner block has its own scope",
      },
      {
        id: "b",
        label: "2 — the inner assignment overrides the outer",
      },
      {
        id: "c",
        label: "undefined — x is shadowed",
      },
      {
        id: "d",
        label: "ReferenceError",
      },
    ],
    correct: "a",
  },
];

export default function Results({ answers, onReset }) {
  const correctIds = QUESTIONS.filter((q) => answers[q.id] === q.correct).map(
    (q) => q.id,
  );
  const score = Math.round((correctIds.length / QUESTIONS.length) * 100);

  // weak area = first concept where the user missed
  const weakConcept =
    QUESTIONS.find((q) => answers[q.id] !== q.correct)?.concept ??
    "None detected";

  const tier = score >= 80 ? "strong" : score >= 50 ? "medium" : "weak";

  const aiMessage =
    tier === "strong"
      ? "You're ready to move on. Your reasoning is consistent and your foundations are solid."
      : tier === "medium"
        ? "There are a few concepts worth reinforcing before progressing — nothing major, just sharpening."
        : "It looks like a few core ideas need to be re-explained more simply. Let's rebuild from there.";

  return (
    <div className="mt-12 grid lg:grid-cols-[1.3fr_1fr] gap-6 animate-fade-in">
      {/* Left: Understanding metrics */}
      <section className="rounded-3xl border border-border bg-bg-card/80 shadow-card p-7 sm:p-9">
        <div className="flex items-center gap-2 text-xs uppercase tracking-[0.2em] text-text-accent">
          <Sparkles className="size-3.5" /> Understanding analysis
        </div>
        <h2 className="mt-3 text-2xl md:text-3xl font-semibold text-text-primary">
          You score is{" "}
          <span className="gradient-cta font-bold text-text-primary p-2 inline-flex items-center justify-center rounded-full w-fit">
            {score}%
          </span>
        </h2>
        <p className="mt-3 text-text-secondary leading-relaxed max-w-xl">
          {aiMessage}
        </p>

        <div className="mt-8 grid sm:grid-cols-3 gap-4">
          {/* <Metric label="Concept understanding" value={`${score}%`} accent />
          <Metric
            label="Practical thinking"
            value={tier === "weak" ? "Developing" : "Strong"}
          />
          <Metric label="Weak area" value={weakConcept} muted /> */}
        </div>

        <div className="mt-8">
          <div className="text-xs uppercase tracking-wider text-text-muted mb-3">
            Per-concept breakdown
          </div>
          <div className="space-y-2.5">
            {QUESTIONS.map((q) => {
              const right = answers[q.id] === q.correct;
              return (
                <div
                  key={q.id}
                  className="flex items-center gap-3 rounded-xl border border-border bg-bg-tertiary/40 px-4 py-3"
                >
                  <span
                    className={`size-2 rounded-full ${
                      right ? "bg-success" : "bg-amber-400"
                    }`}
                  />
                  <div className="flex-1 text-sm text-text-primary">
                    {q.concept}
                  </div>
                  <span
                    className={`text-xs ${
                      right ? "text-success" : "text-amber-300"
                    }`}
                  >
                    {right ? "Understood" : "Needs reinforcement"}
                  </span>
                </div>
              );
            })}
          </div>
        </div>
      </section>

      {/* Right: Recovery plan + next */}
      <section className="space-y-6">
        <NextActionCard tier={"strong"} />

        <button
          onClick={onReset}
          className="w-full inline-flex items-center justify-center gap-2 rounded-xl border border-border bg-bg-card px-5 py-3 text-sm text-text-secondary hover:border-border-strong hover:text-foreground transition-colors"
        >
          <RefreshCw className="size-4" /> Try the assessment again
        </button>
      </section>
    </div>
  );
}

function NextActionCard({ tier }) {
  const map = {
    strong: {
      title: "Continue to Module 3",
      sub: "Functions in depth — closures, currying, composition.",
    },
    medium: {
      title: "Review weak areas",
      sub: "Short focused loop on what wobbled, then continue.",
    },
    weak: {
      title: "Begin guided recovery",
      sub: "Slower, simpler, with the AI walking next to you.",
    },
  };
  const v = map[tier];
  return (
    <div className="rounded-3xl border border-border bg-bg-card p-6">
      <div className="text-xs uppercase tracking-wider text-text-muted">
        Next step
      </div>
      <div className="mt-1 text-lg font-semibold text-text-primary">
        {v.title}
      </div>
      <div className="text-sm text-text-secondary mt-1">{v.sub}</div>
      <Link href="/student/homepage">
        <button className="mt-4 w-full inline-flex items-center justify-center gap-2 rounded-xl gradient-cta px-5 py-3 text-sm font-medium text-white shadow-neon hover:scale-[1.01] transition-transform cursor-pointer">
          Continue learning <ArrowRight className="size-4" />
        </button>
      </Link>
    </div>
  );
}
