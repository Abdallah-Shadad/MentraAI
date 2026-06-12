"use client";

import { useMemo, useState, useEffect } from "react";
import { useRouter } from "@/lib/i18n/navigation";
import { useQueryClient } from "@tanstack/react-query";
import { motion, AnimatePresence } from "framer-motion";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import ErrorState from "@/components/reusable/ErrorState";
import SuccessState from "@/components/reusable/SuccessState";
import { useOnboarding } from "@/hooks/useOnboarding";
import { 
  ChevronLeft, 
  ChevronRight, 
  Check, 
  Sparkles, 
  User, 
  Briefcase, 
  Cpu, 
  Target, 
  Search,
  Plus,
  Minus
} from "lucide-react";

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
  const queryClient = useQueryClient();
  
  // Step management
  const [step, setStep] = useState(0);
  const [direction, setDirection] = useState(1); // 1 = forward, -1 = backward

  const next = () => {
    setDirection(1);
    setStep((s) => Math.min(s + 1, 3));
  };
  const back = () => {
    setDirection(-1);
    setStep((s) => Math.max(s - 1, 0));
  };

  // Answers management
  const [answers, setAnswers] = useState([
    { questionId: 1, answerText: "" },
    { questionId: 2, answerText: "" },
    { questionId: 3, answerText: 0 },
    { questionId: 4, answerText: 0 },
    { questionId: 5, answerText: "" },
    { questionId: 6, answerText: "" },
    { questionId: 7, answerText: "" },
    { questionId: 8, answerText: "" },
    { questionId: 9, answerText: "" },
    { questionId: 10, answerText: [] },
    { questionId: 11, answerText: [] },
  ]);

  // Load draft from localStorage on mount
  useEffect(() => {
    if (typeof window !== "undefined") {
      const savedDraft = localStorage.getItem("mentraai_onboarding_draft");
      if (savedDraft) {
        try {
          const parsed = JSON.parse(savedDraft);
          if (Array.isArray(parsed) && parsed.length === answers.length) {
            setAnswers(parsed);
          }
        } catch (e) {
          console.error("Failed to parse onboarding draft", e);
        }
      }
    }
  }, []);

  // Save draft to localStorage on answers change
  const setAnswer = (id, value) => {
    setAnswers((prev) => {
      const updated = prev.map((answer) =>
        answer.questionId === id ? { ...answer, answerText: value } : answer
      );
      if (typeof window !== "undefined") {
        localStorage.setItem("mentraai_onboarding_draft", JSON.stringify(updated));
      }
      return updated;
    });
  };

  // Keyboard navigation: Enter to proceed or submit
  useEffect(() => {
    const handleKeyDown = (e) => {
      if (e.key === "Enter" && !e.shiftKey) {
        // Prevent enter on textareas or inputs if needed, but for choices it is safe
        const activeEl = document.activeElement;
        if (activeEl && activeEl.tagName === "INPUT" && activeEl.type === "number") {
          activeEl.blur();
        }
        
        // Check if current step answers are valid (optional validation could go here)
        if (step === 3) {
          handleSubmit();
        } else {
          next();
        }
      }
    };
    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [step, answers]);

  // Success flow
  useEffect(() => {
    if (isSuccess) {
      // Clear draft on success
      if (typeof window !== "undefined") {
        localStorage.removeItem("mentraai_onboarding_draft");
      }
      
      // Invalidate user query to synchronize onboarded status across components
      queryClient.invalidateQueries({ queryKey: ["user"] });
      
      const timer = setTimeout(() => {
        if (typeof window !== "undefined") {
          const segments = window.location.pathname.split("/");
          const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "en";
          window.location.href = `/${locale}/student/homepage`;
        }
      }, 1000);
      return () => clearTimeout(timer);
    }
  }, [isSuccess, queryClient]);

  // Submit handler
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

  // Stepper steps configuration
  const stepsConfig = [
    { title: "Personal Info", icon: <User className="w-4 h-4" /> },
    { title: "Career Profile", icon: <Briefcase className="w-4 h-4" /> },
    { title: "AI & Tech", icon: <Cpu className="w-4 h-4" /> },
    { title: "Learning Goals", icon: <Target className="w-4 h-4" /> },
  ];

  // Framer Motion Variants for step slide transition
  const slideVariants = {
    enter: (dir) => ({
      x: dir > 0 ? 150 : -150,
      opacity: 0,
      filter: "blur(4px)",
    }),
    center: {
      x: 0,
      opacity: 1,
      filter: "blur(0px)",
      transition: {
        x: { type: "spring", stiffness: 300, damping: 30 },
        opacity: { duration: 0.2 },
      },
    },
    exit: (dir) => ({
      x: dir > 0 ? -150 : 150,
      opacity: 0,
      filter: "blur(4px)",
      transition: {
        x: { type: "spring", stiffness: 300, damping: 30 },
        opacity: { duration: 0.2 },
      },
    }),
  };

  return (
    <div className="w-full max-w-4xl mx-auto px-4 md:px-8 py-4 min-h-[75vh] flex flex-col justify-between text-foreground">
      {isSuccess && (
        <SuccessState
          close={() => {
            queryClient.invalidateQueries({ queryKey: ["user"] });
            reset();
            if (typeof window !== "undefined") {
              const segments = window.location.pathname.split("/");
              const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "en";
              window.location.href = `/${locale}/student/homepage`;
            }
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

      {/* Stepper Header */}
      <div className="mb-8">
        <div className="flex items-center justify-between relative">
          <div className="absolute left-0 right-0 top-1/2 -translate-y-1/2 h-1 bg-border rounded-full z-0">
            <div 
              className="h-full bg-linear-to-r from-primary to-secondary transition-all duration-300"
              style={{ width: `${(step / 3) * 100}%` }}
            />
          </div>

          {stepsConfig.map((sConfig, index) => {
            const isActive = index === step;
            const isCompleted = index < step;
            return (
              <div key={index} className="flex flex-col items-center z-10">
                <button
                  onClick={() => {
                    setDirection(index > step ? 1 : -1);
                    setStep(index);
                  }}
                  className={`w-10 h-10 rounded-full flex items-center justify-center transition-all duration-300 border-2 cursor-pointer ${
                    isActive
                      ? "bg-background border-primary text-primary shadow-[0_0_15px_rgba(var(--color-primary),0.35)] scale-110"
                      : isCompleted
                      ? "bg-primary border-primary text-white"
                      : "bg-background border-border text-foreground-muted hover:border-primary/50"
                  }`}
                >
                  {isCompleted ? <Check className="w-5 h-5" /> : sConfig.icon}
                </button>
                <span className={`text-xs mt-2 font-medium hidden sm:inline ${isActive ? "text-primary font-semibold" : "text-foreground-muted"}`}>
                  {sConfig.title}
                </span>
              </div>
            );
          })}
        </div>
      </div>

      {/* Content Area with Slide Animation */}
      <div className="flex-1 min-h-[50vh] relative overflow-hidden flex flex-col justify-start py-4">
        <AnimatePresence mode="wait" custom={direction}>
          <motion.div
            key={step}
            custom={direction}
            variants={slideVariants}
            initial="enter"
            animate="center"
            exit="exit"
            className="w-full flex flex-col gap-6"
          >
            {/* Step 0: Personal Info */}
            {step === 0 && (
              <StepContainer title="Personal Background" description="Let's build a foundation. Tell us about your background to help customize your curriculum.">
                <div className="space-y-6">
                  <PremiumChoiceGroup
                    question={questions[0]}
                    value={answers.find((a) => a.questionId === 1)?.answerText}
                    onChange={(v) => setAnswer(1, v)}
                  />

                  <PremiumChoiceGroup
                    question={questions[1]}
                    value={answers.find((a) => a.questionId === 2)?.answerText}
                    onChange={(v) => setAnswer(2, v)}
                  />

                  <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <PremiumNumberField
                      label={questions[2].questionText}
                      value={answers.find((a) => a.questionId === 3)?.answerText || 0}
                      onChange={(v) => setAnswer(3, v)}
                    />
                    <PremiumNumberField
                      label={questions[3].questionText}
                      value={answers.find((a) => a.questionId === 4)?.answerText || 0}
                      onChange={(v) => setAnswer(4, v)}
                    />
                  </div>
                </div>
              </StepContainer>
            )}

            {/* Step 1: Career Profile */}
            {step === 1 && (
              <StepContainer title="Career Profile" description="Your professional preferences directly influence the practical application challenges in your roadmap.">
                <div className="space-y-6">
                  <PremiumChoiceGroup
                    question={questions[4]}
                    value={answers.find((a) => a.questionId === 5)?.answerText}
                    onChange={(v) => setAnswer(5, v)}
                  />
                  <PremiumChoiceGroup
                    question={questions[5]}
                    value={answers.find((a) => a.questionId === 6)?.answerText}
                    onChange={(v) => setAnswer(6, v)}
                  />
                  <PremiumChoiceGroup
                    question={questions[6]}
                    value={answers.find((a) => a.questionId === 7)?.answerText}
                    onChange={(v) => setAnswer(7, v)}
                  />
                  <PremiumChoiceGroup
                    question={questions[7]}
                    value={answers.find((a) => a.questionId === 8)?.answerText}
                    onChange={(v) => setAnswer(8, v)}
                  />
                </div>
              </StepContainer>
            )}

            {/* Step 2: AI & Tech Usage */}
            {step === 2 && (
              <StepContainer title="AI & Current Technologies" description="Identify the tech stack you're comfortable with and your familiarity with AI workflows.">
                <div className="space-y-6">
                  <PremiumChoiceGroup
                    question={questions[8]}
                    value={answers.find((a) => a.questionId === 9)?.answerText}
                    onChange={(v) => setAnswer(9, v)}
                  />
                  <PremiumMultiSelect
                    question={questions[9]}
                    value={answers.find((a) => a.questionId === 10)?.answerText || []}
                    onChange={(v) => setAnswer(10, v)}
                  />
                </div>
              </StepContainer>
            )}

            {/* Step 3: Learning Goals */}
            {step === 3 && (
              <StepContainer title="Future Learning Goals" description="Select the skills, programming languages, and architectures you want to master.">
                <PremiumMultiSelect
                  question={questions[10]}
                  value={answers.find((a) => a.questionId === 11)?.answerText || []}
                  onChange={(v) => setAnswer(11, v)}
                />
              </StepContainer>
            )}
          </motion.div>
        </AnimatePresence>
      </div>

      {/* Navigation Controls */}
      <div className="border-t border-border mt-8 pt-6 flex justify-between items-center backdrop-blur-md bg-background/5 sticky bottom-0 z-30">
        <Button
          variant="outline"
          onClick={back}
          disabled={step === 0}
          className="border-border hover:bg-muted/50 cursor-pointer transition-all duration-200"
        >
          <ChevronLeft className="w-4 h-4 mr-2" />
          Back
        </Button>

        <span className="text-sm font-medium text-foreground-muted">
          Step {step + 1} of 4
        </span>

        <Button
          onClick={step === 3 ? handleSubmit : next}
          disabled={isPending}
          className={`relative cursor-pointer transition-all duration-300 font-semibold px-8 overflow-hidden bg-primary text-white hover:bg-primary/90 hover:scale-[1.02] active:scale-[0.98] ${
            step === 3 ? "bg-gradient-to-r from-primary to-secondary px-12" : ""
          }`}
        >
          {isPending ? (
            <span className="flex items-center gap-2">
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce" />
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce delay-100" />
              <span className="w-1.5 h-1.5 bg-white rounded-full animate-bounce delay-200" />
            </span>
          ) : step === 3 ? (
            <span className="flex items-center gap-2">
              <Sparkles className="w-4 h-4" />
              Generate My Track
            </span>
          ) : (
            <span className="flex items-center gap-1">
              Next
              <ChevronRight className="w-4 h-4" />
            </span>
          )}
        </Button>
      </div>
    </div>
  );
}

/* ===========================
   STEP CONTAINER
=========================== */
function StepContainer({ title, description, children }) {
  return (
    <div className="w-full space-y-4">
      <div className="mb-2">
        <h2 className="text-2xl md:text-3xl font-bold tracking-tight text-foreground bg-clip-text bg-gradient-to-r from-foreground to-foreground-light">
          {title}
        </h2>
        {description && (
          <p className="text-foreground-muted text-sm mt-1.5 leading-relaxed">
            {description}
          </p>
        )}
      </div>
      <div className="space-y-6">{children}</div>
    </div>
  );
}

/* ===========================
   PREMIUM CHOICE GROUP
=========================== */
function PremiumChoiceGroup({ question, value, onChange }) {
  return (
    <div className="space-y-3">
      <h3 className="text-foreground font-semibold text-base md:text-lg">
        {question.questionText}
      </h3>
      <div className="grid grid-cols-1 md:grid-cols-2 gap-3">
        {question.options.map((opt) => {
          const isSelected = value === opt;
          return (
            <motion.div
              key={opt}
              onClick={() => onChange(opt)}
              whileHover={{ scale: 1.01 }}
              whileTap={{ scale: 0.99 }}
              className={`p-4 rounded-xl border-2 cursor-pointer transition-all duration-200 flex items-center justify-between ${
                isSelected
                  ? "border-primary bg-primary/5 shadow-[0_0_15px_rgba(var(--color-primary),0.1)]"
                  : "border-border/60 bg-card/45 hover:border-primary/50 hover:bg-card/75"
              }`}
            >
              <div className="flex items-center gap-3">
                <div className={`w-5 h-5 rounded-full border-2 flex items-center justify-center shrink-0 transition-all ${
                  isSelected ? "border-primary text-primary" : "border-border"
                }`}>
                  {isSelected && <div className="w-2.5 h-2.5 rounded-full bg-primary" />}
                </div>
                <span className="text-foreground font-medium text-sm md:text-base leading-snug">
                  {opt}
                </span>
              </div>
            </motion.div>
          );
        })}
      </div>
    </div>
  );
}

/* ===========================
   PREMIUM NUMBER FIELD
=========================== */
function PremiumNumberField({ label, value, onChange }) {
  const increment = () => onChange(Math.max(0, value + 1));
  const decrement = () => onChange(Math.max(0, value - 1));

  return (
    <div className="space-y-3 p-5 rounded-xl border border-border/60 bg-card/30 flex flex-col justify-center">
      <h3 className="text-foreground font-semibold text-sm md:text-base mb-1">
        {label}
      </h3>
      <div className="flex items-center gap-4">
        <button
          type="button"
          onClick={decrement}
          className="w-10 h-10 rounded-lg border border-border hover:border-primary flex items-center justify-center transition-all bg-card/60 cursor-pointer"
        >
          <Minus className="w-4 h-4" />
        </button>
        <input
          type="number"
          value={value}
          min="0"
          onChange={(e) => onChange(Math.max(0, Number(e.target.value)))}
          className="flex-1 text-center p-3 text-lg font-bold rounded-lg border border-border bg-background/50 focus:border-primary focus:outline-hidden"
        />
        <button
          type="button"
          onClick={increment}
          className="w-10 h-10 rounded-lg border border-border hover:border-primary flex items-center justify-center transition-all bg-card/60 cursor-pointer"
        >
          <Plus className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}

/* ===========================
   PREMIUM MULTI SELECT
=========================== */
function PremiumMultiSelect({ question, value, onChange }) {
  const [searchQuery, setSearchQuery] = useState("");
  
  const toggle = (item) => {
    if (value.includes(item)) {
      onChange(value.filter((v) => v !== item));
    } else {
      onChange([...value, item]);
    }
  };

  const filteredOptions = useMemo(() => {
    return question.options.filter(opt => 
      opt.toLowerCase().includes(searchQuery.toLowerCase())
    );
  }, [question.options, searchQuery]);

  return (
    <div className="space-y-3">
      <h3 className="text-foreground font-semibold text-base md:text-lg">
        {question.questionText}
      </h3>
      
      {/* Search Filter */}
      <div className="relative w-full max-w-md">
        <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-foreground-muted" />
        <input
          type="text"
          placeholder="Filter skills..."
          value={searchQuery}
          onChange={(e) => setSearchQuery(e.target.value)}
          className="w-full pl-9 pr-4 py-2 border border-border/80 rounded-lg bg-background/50 text-sm focus:outline-hidden focus:border-primary"
        />
      </div>

      {/* Grid of tags */}
      <div className="flex flex-wrap gap-2 max-h-[300px] overflow-y-auto p-4 border border-border/60 bg-card/30 rounded-xl scrollbar-thin">
        {filteredOptions.length === 0 ? (
          <p className="text-sm text-foreground-muted italic py-4">No matching skills found.</p>
        ) : (
          filteredOptions.map((opt) => {
            const isSelected = value.includes(opt);
            return (
              <Badge
                key={opt}
                onClick={() => toggle(opt)}
                className={`cursor-pointer px-4 py-2 text-sm font-medium border transition-all rounded-lg select-none ${
                  isSelected
                    ? "bg-primary border-primary text-white hover:bg-primary/90"
                    : "bg-background border-border text-foreground hover:border-primary/50 hover:bg-muted/30"
                }`}
              >
                {isSelected && <Check className="w-3.5 h-3.5 mr-1 inline-block" />}
                {opt}
              </Badge>
            );
          })
        )}
      </div>
    </div>
  );
}

/* ===========================
   DATA (Onboarding Questions)
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
