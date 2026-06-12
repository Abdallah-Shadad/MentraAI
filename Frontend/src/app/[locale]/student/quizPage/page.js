"use client";

import { Suspense, useEffect } from "react";
import { useSearchParams } from "next/navigation";
import { useQuizEngine } from "./hooks/useQuizEngine";
import { useGetCurrentRoadmap } from "@/hooks/useRoadmap";
import Header from "./_sections/Header";
import ProgressBar from "./_sections/ProgressBar";
import QuestionCard from "./_sections/QuestionCard";
import SidePanel from "./_sections/SidePanel";
import NavBar from "./_sections/NavBar";
import Analyzing from "./_sections/Analyzing";
import QuizResults from "./_sections/QuizResults";
import NeuralBackdrop from "./_sections/NeuralBackdrop";
import { Loader2, BrainCircuit, AlertCircle } from "lucide-react";
import { Button } from "@/components/ui/button";

function QuizPageInner() {
  const searchParams = useSearchParams();
  const stageProgressId = searchParams.get("stageProgressId");

  const { data: currentRoadmapData } = useGetCurrentRoadmap();
  const stages = currentRoadmapData?.data?.stages || [];
  const currentStage = stages.find((s) => s.stageProgressId === stageProgressId);
  const stageName = currentStage?.stageName || "";

  const {
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
  } = useQuizEngine(stageProgressId);

  // Initialize quiz on mount or stageProgressId change
  useEffect(() => {
    if (stageProgressId) {
      initQuiz(stageProgressId);
    }
  }, [stageProgressId, initQuiz]);

  if (!stageProgressId) {
    return (
      <div className="min-h-screen w-full flex items-center justify-center bg-background text-foreground p-6">
        <div className="max-w-md text-center space-y-4">
          <AlertCircle className="w-12 h-12 text-destructive mx-auto" />
          <h2 className="text-xl font-bold">Missing Stage ID</h2>
          <p className="text-foreground-muted text-sm">
            Could not start the assessment because no stage identifier was provided in the link.
          </p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background bg-neural relative overflow-hidden text-foreground">
      <NeuralBackdrop />
      <div className="absolute top-0 left-0 w-[600px] h-[600px] bg-linear-to-br from-primary/10 to-secondary/10 rounded-full blur-3xl"></div>

      <div className="relative max-w-6xl mx-auto px-5 sm:px-8 py-8 lg:py-12">
        <Header 
          stageName={stageName} 
          totalQuestions={total} 
          timeLimitMinutes={totalTime ? Math.round(totalTime / 60) : 0} 
        />

        {/* Phase: Generating / Loading */}
        {phase === "generating" && (
          <div className="mt-16 flex flex-col items-center justify-center gap-4 py-12">
            <div className="relative">
              <div className="w-20 h-20 rounded-3xl bg-primary/10 flex items-center justify-center border border-primary/20 animate-pulse">
                <BrainCircuit className="w-10 h-10 text-primary" />
              </div>
              <Loader2 className="w-6 h-6 text-primary animate-spin absolute -top-1 -right-1" />
            </div>
            <div className="text-center">
              <h3 className="text-lg font-bold">Constructing Adaptive Assessment</h3>
              <p className="text-foreground-muted text-sm mt-1 max-w-xs leading-relaxed">
                Our AI Agent is compiling stage-specific questions to check your knowledge gaps...
              </p>
            </div>
          </div>
        )}

        {/* Phase: Active Quiz */}
        {phase === "quiz" && (
          <div className="mt-8 flex flex-col-reverse lg:flex-row gap-8">
            <div className="flex-1">
              <ProgressBar
                progress={progress}
                answered={answeredCount}
                total={total}
              />

              <QuestionCard
                q={question}
                index={current}
                total={total}
                selected={answers[question?.id]}
                onSelect={selectAnswer}
                unlockedHints={unlockedHints[question?.id] || []}
                hintLoading={hintLoading}
                onUnlockHint={() => requestHint(question?.id)}
              />

              <NavBar
                current={current}
                total={total}
                hasAnswer={!!answers[question?.id]}
                onPrev={prev}
                onNext={next}
                onAnalyze={next} // on last question, next submits answers
                answeredAll={answeredCount === total}
              />
            </div>

            <SidePanel timeRemaining={timeRemaining} totalTime={totalTime} />
          </div>
        )}

        {/* Phase: Submitting & Analysis */}
        {phase === "submitting" && <Analyzing step={analyzeStep} />}

        {/* Phase: Results */}
        {phase === "results" && (
          <QuizResults
            submitResult={submitResult}
            onReset={reset}
            stageProgressId={stageProgressId}
          />
        )}

        {/* Phase: Error / Exception */}
        {phase === "error" && (
          <div className="mt-16 rounded-2xl border border-destructive/20 bg-destructive/5 p-8 max-w-xl mx-auto text-center space-y-6">
            <AlertCircle className="w-12 h-12 text-destructive mx-auto animate-bounce" />
            <div>
              <h3 className="text-lg font-bold text-foreground">Assessment Initialization Failed</h3>
              <p className="text-sm text-foreground-muted mt-2 leading-relaxed">
                {errorMessage}
              </p>
            </div>
            <div className="flex justify-center gap-3">
              <Button onClick={reset} className="bg-primary hover:bg-primary/90 text-white font-semibold cursor-pointer">
                Try Reconnecting
              </Button>
              <Button variant="outline" onClick={() => window.history.back()} className="border-border hover:bg-muted/40 cursor-pointer text-foreground">
                Go Back
              </Button>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}

export default function QuizPage() {
  return (
    <Suspense 
      fallback={
        <div className="min-h-screen w-full flex items-center justify-center bg-background text-foreground">
          <div className="flex flex-col items-center gap-4">
            <Loader2 className="w-8 h-8 text-primary animate-spin" />
            <p className="text-foreground-muted text-sm font-medium">Preparing space...</p>
          </div>
        </div>
      }
    >
      <QuizPageInner />
    </Suspense>
  );
}
