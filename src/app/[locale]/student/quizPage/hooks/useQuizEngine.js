"use client";

import { useState } from "react";

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

export function useQuizEngine() {
  const [phase, setPhase] = useState("quiz");
  const [current, setCurrent] = useState(0);
  const [answers, setAnswers] = useState({});
  const [analyzeStep, setAnalyzeStep] = useState(0);

  const question = QUESTIONS[current];

  //total questions
  const total = QUESTIONS.length;
  //answered questions
  const answeredCount = Object.keys(answers).length;
  //progress
  const progress = Math.round((answeredCount / total) * 100);

  //select answer = put the answer option id in the answers object
  const selectAnswer = (optionId) => {
    setAnswers((prev) => ({
      ...prev,
      [question.id]: optionId,
    }));
  };

  //navBar
  const next = () => {
    if (current < total - 1) {
      setCurrent((prev) => prev + 1);
      return;
    }

    startAnalysis();
  };

  const prev = () => {
    if (current > 0) {
      setCurrent((prev) => prev - 1);
    }
  };

  const startAnalysis = () => {
    setPhase("analyzing");
    setAnalyzeStep(0);

    [0, 1, 2, 3].forEach((step) => {
      setTimeout(() => {
        setAnalyzeStep(step);
      }, step * 850);
    });

    setTimeout(() => {
      setPhase("results");
    }, 3800);
  };

  const reset = () => {
    setAnswers({});
    setCurrent(0);
    setPhase("quiz");
  };

  return {
    //progressBar
    progress,
    answeredCount,
    total,
    //question Card
    question,
    current,
    answers,
    selectAnswer,
    //navBar
    next,
    prev,
    startAnalysis,

    //Analysis
    phase,
    analyzeStep,
    reset,
  };
}
