"use client";

export default function ProgressBar({ progress, answered, total }) {
  /**
   * progress: number - percentage of progress
   * answered: number - number of answered questions
   * total: number - total number of questions
   */

  return (
    <div className="mb-4 border border-primary bg-primary/5 backdrop-blur-lg rounded-3xl p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="text-sm text-text-secondary">
          <span className="text-foreground font-medium">{answered}</span> of{" "}
          {total} questions explored
        </div>
      </div>
      <div className="mt-3 h-2 rounded-full bg-primary/20 overflow-hidden">
        <div
          className="h-full bg-linear-to-r from-primary to-secondary transition-all duration-500"
          style={{ width: `${progress}%` }}
        />
      </div>
    </div>
  );
}
