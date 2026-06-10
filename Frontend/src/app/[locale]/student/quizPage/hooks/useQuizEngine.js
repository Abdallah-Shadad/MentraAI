"use client";

import { useState } from "react";

// questions data
export const QUESTIONS = [
  {
    id: 1,
    concept: "Variables & Scope",
    prompt:
      "What value will be logged? `let x = 1; { let x = 2; } console.log(x);`",
    options: [
      { id: "a", label: "1" },
      { id: "b", label: "2" },
      { id: "c", label: "undefined" },
      { id: "d", label: "ReferenceError" },
    ],
    correct: "a",
  },
  {
    id: 2,
    concept: "Variables",
    prompt:
      "Which keyword allows reassignment but prevents redeclaration in the same scope?",
    options: [
      { id: "a", label: "var" },
      { id: "b", label: "const" },
      { id: "c", label: "let" },
      { id: "d", label: "static" },
    ],
    correct: "c",
  },
  {
    id: 3,
    concept: "Constants",
    prompt: "What happens if you try to reassign a `const` variable?",
    options: [
      { id: "a", label: "Nothing happens" },
      { id: "b", label: "It becomes undefined" },
      { id: "c", label: "A TypeError is thrown" },
      { id: "d", label: "The value changes normally" },
    ],
    correct: "c",
  },
  {
    id: 4,
    concept: "Data Types",
    prompt: "What is the result of `typeof null`?",
    options: [
      { id: "a", label: "'null'" },
      { id: "b", label: "'object'" },
      { id: "c", label: "'undefined'" },
      { id: "d", label: "'boolean'" },
    ],
    correct: "b",
  },
  {
    id: 5,
    concept: "Equality",
    prompt: "What does `===` check in JavaScript?",
    options: [
      { id: "a", label: "Value only" },
      { id: "b", label: "Type only" },
      { id: "c", label: "Value and type" },
      { id: "d", label: "Reference only" },
    ],
    correct: "c",
  },
  {
    id: 6,
    concept: "Arrays",
    prompt: "Which method adds an item to the end of an array?",
    options: [
      { id: "a", label: "push()" },
      { id: "b", label: "pop()" },
      { id: "c", label: "shift()" },
      { id: "d", label: "unshift()" },
    ],
    correct: "a",
  },
  {
    id: 7,
    concept: "Functions",
    prompt: "How do you define an arrow function?",
    options: [
      { id: "a", label: "function => {}" },
      { id: "b", label: "() => {}" },
      { id: "c", label: "=> function {}" },
      { id: "d", label: "arrow() {}" },
    ],
    correct: "b",
  },
  {
    id: 8,
    concept: "Objects",
    prompt: "How do you access the `name` property of an object `user`?",
    options: [
      { id: "a", label: "user->name" },
      { id: "b", label: "user:name" },
      { id: "c", label: "user.name" },
      { id: "d", label: "user[name]" },
    ],
    correct: "c",
  },
  {
    id: 9,
    concept: "Loops",
    prompt: "Which loop is commonly used to iterate over array elements?",
    options: [
      { id: "a", label: "for" },
      { id: "b", label: "while" },
      { id: "c", label: "do...while" },
      { id: "d", label: "All of the above" },
    ],
    correct: "d",
  },
  {
    id: 10,
    concept: "Promises",
    prompt: "Which keyword is used with Promises to wait for a result?",
    options: [
      { id: "a", label: "pause" },
      { id: "b", label: "await" },
      { id: "c", label: "yield" },
      { id: "d", label: "hold" },
    ],
    correct: "b",
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
