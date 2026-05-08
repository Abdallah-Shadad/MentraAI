"use client";
import { ArrowLeft, ArrowRight, Brain } from "lucide-react";

export default function NavBar({
  current,
  total,
  hasAnswer,
  onPrev,
  onNext,
  onAnalyze,
  answeredAll,
}) {
  const isLast = current === total - 1;
  return (
    <div className="mt-8 flex items-center justify-between gap-2">
      <button
        onClick={onPrev}
        disabled={current === 0}
        className="inline-flex items-center gap-2 rounded-xl border border-border bg-bg-card px-5 py-3 md:text-sm text-xs text-text-secondary transition-colors hover:border-border-strong hover:text-foreground disabled:opacity-40 disabled:cursor-not-allowed"
      >
        <ArrowLeft className="size-4" /> Previous
      </button>

      {isLast ? (
        <button
          onClick={onAnalyze}
          disabled={!answeredAll}
          className="inline-flex items-center gap-2 gradient-cta px-6 py-3 md:text-sm text-xs font-medium text-white shadow-neon transition-transform hover:scale-[1.02] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 cursor-pointer rounded-xl"
        >
          <Brain className="size-4" /> Analyze My Understanding
        </button>
      ) : (
        <button
          onClick={onNext}
          disabled={!hasAnswer}
          className="inline-flex items-center gap-2 rounded-xl bg-primary-dark px-6 py-3 md:text-sm text-xs font-medium text-white shadow-neon transition-transform hover:scale-[1.02] disabled:opacity-50 disabled:cursor-not-allowed disabled:hover:scale-100 cursor-pointer"
        >
          Next question <ArrowRight className="size-4" />
        </button>
      )}
    </div>
  );
}
