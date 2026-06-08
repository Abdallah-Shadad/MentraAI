"use client";
import { useQuizEngine } from "./hooks/useQuizEngine";
import Header from "./_sections/Header";
import ProgressBar from "./_sections/ProgressBar";
import QuestionCard from "./_sections/QuestionCard";
import SidePanel from "./_sections/SidePanel";
import NavBar from "./_sections/NavBar";
import Analyzing from "./_sections/Analyzing";
import Results from "./_sections/Results";
import NeuralBackdrop from "./_sections/NeuralBackdrop";

export default function QuizPage() {
  const {
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
    analyzeStep,
    reset,
    phase,
  } = useQuizEngine();

  return (
    <div className="min-h-screen bg-background bg-neural relative overflow-hidden">
      <NeuralBackdrop />
      <div className="absolute top-0 left-0 w-[600px] h-[600px] bg-linear-to-br from-primary/10 to-secondary/10 rounded-full blur-3xl"></div>

      <div className="relative max-w-6xl mx-auto px-5 sm:px-8 py-8 lg:py-12">
        <Header />

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
                selected={answers[question.id]}
                onSelect={selectAnswer}
              />

              <NavBar
                current={current}
                total={total}
                hasAnswer={!!answers[question.id]}
                onPrev={prev}
                onNext={next}
                onAnalyze={startAnalysis}
                answeredAll={answeredCount === total}
              />
            </div>

            <SidePanel />
          </div>
        )}

        {phase === "analyzing" && <Analyzing step={analyzeStep} />}

        {phase === "results" && <Results answers={answers} onReset={reset} />}
      </div>
    </div>
  );
}
