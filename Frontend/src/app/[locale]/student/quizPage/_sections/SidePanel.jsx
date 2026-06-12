"use client";
import { Clock, Brain, Sparkles } from "lucide-react";
import QuizTimer from "./QuizTimer";

export default function SidePanel({ timeRemaining, totalTime }) {
  return (
    <aside className="w-full lg:w-80 space-y-4 shrink-0">
      <div className="bg-card/25 border border-border rounded-2xl p-5 backdrop-blur-lg">
        <div className="flex items-center gap-2 text-xs uppercase tracking-wider text-foreground-muted mb-4">
          <Clock className="size-3.5" /> Time Remaining
        </div>
        
        <div className="flex justify-center my-6">
          <QuizTimer timeRemaining={timeRemaining} totalTime={totalTime} />
        </div>
        
        <div className="text-xs text-foreground-muted mt-4 pt-3 border-t border-border/40">
          <Sparkles className="size-3.5 inline mr-1 text-primary animate-pulse" />
          Stay focused, stay calm. The quiz will auto-submit when time runs out.
        </div>
      </div>
    </aside>
  );
}
