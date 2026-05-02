"use client";

import { Progress } from "@/components/ui/progress";

export default function TrackHeader({
  title,
  description,
  progress,
  estimatedWeeks,
  totalModules,
  completedModules,
}) {
  return (
    <div className="relative overflow-hidden rounded-xl border border-border bg-linear-to-br p-4 md:p-6 from-slate-900 to-slate-800 shadow-xl">
      {/* Background decoration */}
      <div className="absolute top-0 right-0 w-64 h-64 bg-linear-to-bl from-primary/10 to-transparent rounded-full blur-3xl -translate-y-1/2 translate-x-1/2" />
      <div className="absolute bottom-0 left-0 w-48 h-48 bg-linear-to-tr from-secondary/10 to-transparent rounded-full blur-2xl translate-y-1/2 -translate-x-1/2" />

      <div className="relative z-10">
        {/* Title */}
        <div className="mb-6">
          <span className="text-xs font-medium text-primary uppercase tracking-wider mb-2 block">
            Learning Track
          </span>

          <h1 className="text-2xl md:text-3xl font-heading font-bold text-text-foreground mb-2">
            {title}
          </h1>

          <p className="text-text-muted text-sm md:text-base max-w-2xl">
            {description}
          </p>
        </div>

        {/* Progress */}
        <div className="grid grid-cols-1 md:grid-cols-3 gap-6 items-end">
          {/* Progress bar */}
          <div className="md:col-span-2">
            <div className="flex items-center justify-between mb-2">
              <span className="text-sm text-text-muted">Overall Progress</span>
              <span className="text-sm font-semibold text-text-foreground">
                {progress}%
              </span>
            </div>

            <div className="relative">
              <Progress value={progress} className="h-3 bg-bg-muted!" />

              <div
                className="absolute top-0 left-0 h-3 rounded-full bg-linear-to-r from-primary to-secondary transition-all duration-500"
                style={{ width: `${progress}%` }}
              />
            </div>

            <div className="flex items-center justify-between mt-2">
              <span className="text-xs text-text-muted">
                {completedModules} of {totalModules} modules completed
              </span>
            </div>
          </div>

          {/* Estimated time */}
          <div className="flex flex-col items-start md:items-end">
            <div className="bg-bg-card/50 rounded-lg px-4 py-3 border border-primary/20">
              <span className="text-xs text-text-muted block mb-1">
                Est. Completion
              </span>

              <span className="text-lg font-heading font-semibold text-text-foreground">
                ~{estimatedWeeks} weeks
              </span>
            </div>
          </div>
        </div>

        {/* AI Insight */}
        <div className="mt-6 p-4 rounded-lg bg-primary/5 border border-primary/20">
          <p className="text-sm text-text-muted">
            <span className="text-text-foreground font-medium">
              AI Insight:{" "}
            </span>
            Based on your current performance, you are approximately{" "}
            {estimatedWeeks} weeks away from the advanced stage.
          </p>
        </div>
      </div>
    </div>
  );
}
