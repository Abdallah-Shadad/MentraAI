"use client";
import { Clock, Brain, Sparkles } from "lucide-react";
import { useState, useEffect } from "react";
export default function SidePanel() {
  const [time, setTime] = useState({ hours: 0, minutes: 12, seconds: 0 });
  useEffect(() => {
    const interval = setTimeout(() => {
      if (time.seconds === 0) {
        setTime({ hours: time.hours, minutes: time.minutes - 1, seconds: 59 });
      } else {
        setTime({
          hours: time.hours,
          minutes: time.minutes,
          seconds: time.seconds - 1,
        });
      }
    }, 1000);
  }, [time]);
  return (
    <aside className="space-y-4">
      <div className="bg-bg-card/25 border border-border rounded-2xl p-5 backdrop-blur-lg">
        <div className="flex items-center gap-2 text-xs uppercase tracking-wider text-text-muted">
          <Clock className="size-3.5" /> Timer
        </div>
        <div className="my-8 text-2xl font-semibold text-text-primary">
          <span className="p-4 bg-bg-tertiary rounded-md mx-2">
            {String(time.hours).padStart(2, "0")}
          </span>
          <span className="p-4 bg-bg-tertiary rounded-md mx-2">
            {String(time.minutes).padStart(2, "0")}
          </span>
          <span className="p-4 bg-bg-tertiary rounded-md mx-2">
            {String(time.seconds).padStart(2, "0")}
          </span>
        </div>
        <div className="text-xs text-text-muted mt-1">
          <Sparkles className="size-3.5 inline mr-1" /> Stay focused, stay calm.
        </div>
      </div>
    </aside>
  );
}
