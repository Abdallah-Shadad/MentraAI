"use client";

import { useState, useEffect, useCallback, useRef } from "react";
import { 
  useGenerateQuiz, 
  useSubmitQuiz, 
  useQuizHistory 
} from "@/hooks/useQuiz";
import { getQuiz, getQuestionHint } from "@/services/quiz.service";

export function useQuizEngine(stageProgressId) {
  const [phase, setPhase] = useState("idle"); // idle, generating, quiz, submitting, results, error
  const [questions, setQuestions] = useState([]);
  const [current, setCurrent] = useState(0);
  const [answers, setAnswers] = useState({}); // { questionId: "A" }
  const [timeRemaining, setTimeRemaining] = useState(null);
  const [totalTime, setTotalTime] = useState(null);
  const [quizId, setQuizId] = useState(null);
  const [submitResult, setSubmitResult] = useState(null);
  const [errorMessage, setErrorMessage] = useState("");
  const [analyzeStep, setAnalyzeStep] = useState(0);
  const [unlockedHints, setUnlockedHints] = useState({}); // { questionId: ["hint1", "hint2"] }
  const [hintLoading, setHintLoading] = useState(false);

  // Keep answers in a ref so auto-submit can access the latest state without re-binding the timer effect
  const answersRef = useRef({});
  useEffect(() => {
    answersRef.current = answers;
  }, [answers]);

  const { mutateAsync: generateMutation } = useGenerateQuiz();
  const { mutateAsync: submitMutation } = useSubmitQuiz();
  const { refetch: fetchHistory } = useQuizHistory(stageProgressId, { enabled: false });

  // Initialize and generate/retrieve quiz
  const initQuiz = useCallback(async (id) => {
    if (!id) return;
    setPhase("generating");
    setErrorMessage("");
    try {
      // 1. Try to generate a new quiz
      const res = await generateMutation(id);
      const quizData = res?.data || res;
      loadQuizData(quizData);
    } catch (err) {
      console.warn("Quiz generation check:", err);
      const status = err?.response?.status;
      
      // If 409, there's already an active (unsubmitted) quiz attempt. Let's find it.
      if (status === 409 || err?.response?.data?.error?.code === "QUIZ_PENDING_EXISTS") {
        try {
          const historyRes = await fetchHistory();
          const attempts = historyRes?.data?.data || historyRes?.data || [];
          const activeAttempt = attempts.find(a => !a.isSubmitted);
          
          if (activeAttempt) {
            // Fetch the details of the active attempt
            const existingQuiz = await getQuiz(activeAttempt.quizId);
            const quizData = existingQuiz?.data || existingQuiz;
            loadQuizData(quizData);
            return;
          }
        } catch (historyErr) {
          console.error("Failed to restore existing quiz attempt:", historyErr);
        }
      }
      
      // Generic error handling
      setErrorMessage(
        err?.response?.data?.error?.message || 
        "Failed to generate quiz. Please verify if the stage is unlocked."
      );
      setPhase("error");
    }
  }, [generateMutation, fetchHistory]);

  const loadQuizData = (quizData) => {
    if (!quizData || !quizData.quizId) {
      setErrorMessage("Received invalid quiz payload from server.");
      setPhase("error");
      return;
    }
    setQuizId(quizData.quizId);
    setQuestions(quizData.questions || []);
    setCurrent(0);
    
    // Check if session backup exists for this specific quiz
    let restoredAnswers = {};
    let restoredTime = null;
    if (typeof window !== "undefined") {
      const saved = localStorage.getItem(`mentra_quiz_backup_${quizData.quizId}`);
      if (saved) {
        try {
          const parsed = JSON.parse(saved);
          restoredAnswers = parsed.answers || {};
          restoredTime = parsed.timeRemaining;
        } catch (e) {
          console.error("Failed to load quiz session backup", e);
        }
      }
    }
    
    setAnswers(restoredAnswers);

    // Setup time limit
    const limitMinutes = quizData.timeLimitMinutes || 10;
    const limitSeconds = limitMinutes * 60;
    
    if (restoredTime !== null && restoredTime > 0) {
      setTimeRemaining(restoredTime);
    } else {
      setTimeRemaining(limitSeconds);
    }
    setTotalTime(limitSeconds);
    setPhase("quiz");
  };

  // Session backup save
  useEffect(() => {
    if (phase === "quiz" && quizId) {
      if (typeof window !== "undefined") {
        localStorage.setItem(
          `mentra_quiz_backup_${quizId}`,
          JSON.stringify({ answers, timeRemaining })
        );
      }
    }
  }, [answers, timeRemaining, phase, quizId]);

  // Answer selection
  const selectAnswer = (label) => {
    setAnswers((prev) => ({
      ...prev,
      [questions[current].id]: label,
    }));
  };

  // Nav actions
  const next = () => {
    if (current < questions.length - 1) {
      setCurrent((prev) => prev + 1);
    } else {
      submitAnswers();
    }
  };

  const prev = () => {
    if (current > 0) {
      setCurrent((prev) => prev - 1);
    }
  };

  // Submit answers to API
  const submitAnswers = useCallback(async () => {
    if (!quizId) return;
    setPhase("submitting");
    setErrorMessage("");

    // Simulate analysis transitions
    setAnalyzeStep(0);
    const stepInterval = setInterval(() => {
      setAnalyzeStep((s) => Math.min(s + 1, 3));
    }, 800);

    try {
      // Map local answers map into the backend answers DTO list: [{ questionId, answer }]
      const formattedAnswers = questions.map((q) => ({
        questionId: q.id,
        answer: answersRef.current[q.id] || "", // Send blank string if unanswered
      }));

      const res = await submitMutation({
        quizId,
        answers: formattedAnswers,
      });

      // Clear session backup
      if (typeof window !== "undefined") {
        localStorage.removeItem(`mentra_quiz_backup_${quizId}`);
      }

      clearInterval(stepInterval);
      setAnalyzeStep(3);
      setSubmitResult(res);
      setPhase("results");
    } catch (err) {
      clearInterval(stepInterval);
      setErrorMessage(
        err?.response?.data?.error?.message || 
        "Failed to submit your answers. Please check your connection."
      );
      setPhase("error");
    }
  }, [quizId, questions, submitMutation]);

  // Timer Countdown Effect
  useEffect(() => {
    if (phase !== "quiz" || timeRemaining === null) return;
    if (timeRemaining <= 0) {
      submitAnswers();
      return;
    }

    const interval = setInterval(() => {
      setTimeRemaining((prev) => {
        if (prev <= 1) {
          clearInterval(interval);
          return 0;
        }
        return prev - 1;
      });
    }, 1000);

    return () => clearInterval(interval);
  }, [phase, timeRemaining, submitAnswers]);
 
  // Request a hint for a specific question
  const requestHint = useCallback(async (questionId) => {
    if (!quizId || !questionId) return;
    const currentHints = unlockedHints[questionId] || [];
    const nextIndex = currentHints.length;
    if (nextIndex >= 3) return; // limit to 3 hints (levels 1, 2, 3)

    setHintLoading(true);
    try {
      const res = await getQuestionHint(quizId, questionId, nextIndex);
      const hintText = res?.data?.hint || res?.hint;
      if (hintText) {
        setUnlockedHints((prev) => ({
          ...prev,
          [questionId]: [...(prev[questionId] || []), hintText],
        }));
      }
    } catch (err) {
      console.error("Failed to fetch question hint:", err);
    } finally {
      setHintLoading(false);
    }
  }, [quizId, unlockedHints]);

  // Reset engine for retries
  const reset = () => {
    setAnswers({});
    setSubmitResult(null);
    setQuestions([]);
    setCurrent(0);
    setTimeRemaining(null);
    setTotalTime(null);
    setQuizId(null);
    setErrorMessage("");
    setUnlockedHints({});
    setHintLoading(false);
    initQuiz(stageProgressId);
  };

  // Compute stats
  const total = questions.length;
  const answeredCount = Object.keys(answers).length;
  const progress = total > 0 ? Math.round((answeredCount / total) * 100) : 0;
  const question = questions[current] || null;

  return {
    phase,
    progress,
    answeredCount,
    total,
    question,
    current,
    answers,
    timeRemaining,
    totalTime,
    submitResult,
    errorMessage,
    analyzeStep,
    unlockedHints,
    hintLoading,
    requestHint,
    selectAnswer,
    next,
    prev,
    reset,
    initQuiz,
  };
}
