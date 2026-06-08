"use client";

import { Progress } from "@/components/ui/progress";
import { BookOpen, CheckCircle2 } from "lucide-react";

export default function TrackHeader({
  title,
  description,
  progress,
  estimatedWeeks,
  totalModules,
  completedModules,
  totalLessons,
  completedLessons,
}) {
  return (
    <div className="relative overflow-hidden rounded-3xl border border-border bg-linear-to-br p-4 md:p-6 from-background to-card shadow-xl">
      {/* Background decoration */}
      <div className="absolute top-0 right-0 w-64 h-64 bg-linear-to-bl from-primary/10 to-transparent rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
      <div className="absolute bottom-0 left-0 w-48 h-48 bg-linear-to-tr from-secondary/10 to-transparent rounded-full blur-2xl translate-y-1/2 -translate-x-1/2" />

      <div className="relative z-10">
        {/* Title */}
        <div className="mb-6">
          <span className="text-xs font-medium text-foreground uppercase tracking-wider mb-2 block">
            Learning Track
          </span>

          <h1 className="text-2xl md:text-3xl font-heading font-bold text-foreground mb-2">
            {title}
          </h1>

          <p className="text-foreground-muted text-sm md:text-base max-w-2xl">
            {description}
          </p>
        </div>

        {/* Progress */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6 items-center">
          {/* Progress bar */}
          <div className="md:col-span-2">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-foreground-muted">
                Overall Progress
              </span>
              <span className="text-sm font-semibold text-foreground">
                {progress}%
              </span>
            </div>

            <div className="relative">
              <Progress
                value={progress}
                className="h-3 bg-foreground-muted/20"
              />

              <div
                className="absolute top-0 left-0 h-3 rounded-full bg-linear-to-r from-primary to-secondary transition-all duration-500"
                style={{ width: `${progress}%` }}
              />
            </div>
          </div>
          {/* progress details */}
          <div className="col-span-2 flex flex-col md:flex-row gap-2">
            {/* modules progress */}
            <div className="flex items-center justify-between gap-4 border-l border-border p-4">
              <span className="p-4 border border-accent rounded-md bg-accent">
                <BookOpen />
              </span>
              <div className="flex flex-col gap-1">
                <span className="text-foreground-muted">Modules</span>
                <span className="text-xl font-semibold text-foreground">
                  {completedModules} of {totalModules}
                </span>
              </div>
            </div>
            {/* completed lessons */}
            <div className="flex items-center justify-between gap-4 border-l border-border p-4">
              <span className="p-4 border border-success text-success rounded-md bg-success/20">
                <CheckCircle2 />
              </span>
              <div className="flex flex-col gap-1">
                <span className="text-foreground-muted">Lessons</span>
                <span className="text-xl font-semibold text-foreground">
                  {completedLessons} of {totalLessons}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
