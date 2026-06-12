"use client";
import { Clock } from "lucide-react";

export default function QuizTimer({ timeRemaining, totalTime }) {
  if (timeRemaining === null || timeRemaining === undefined) return null;

  const minutes = Math.floor(timeRemaining / 60);
  const seconds = timeRemaining % 60;
  const formattedTime = `${String(minutes).padStart(2, "0")}:${String(seconds).padStart(2, "0")}`;

  const percentage = totalTime > 0 ? (timeRemaining / totalTime) * 100 : 100;

  // Determine color based on time remaining percentage
  let colorClass = "text-success bg-success/10 border-success/20";
  if (percentage <= 20) {
    colorClass = "text-destructive bg-destructive/10 border-destructive/20 animate-pulse";
  } else if (percentage <= 50) {
    colorClass = "text-amber-500 bg-amber-500/10 border-amber-500/20";
  }

  return (
    <div className={`flex items-center gap-2 px-3.5 py-1.5 rounded-full border text-xs font-mono font-bold ${colorClass}`}>
      <Clock className="w-3.5 h-3.5" />
      <span>{formattedTime}</span>
    </div>
  );
}
