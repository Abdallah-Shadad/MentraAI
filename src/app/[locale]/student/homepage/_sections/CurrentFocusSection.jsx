"use client";

import { ArrowRight, BookOpen, Clock, Zap } from "lucide-react";
import { Button } from "@/components/ui/button";

const CurrentFocusSection = ({
  moduleName,
  progress,
  lastLesson,
  totalLessons,
  completedLessons,
  weakPoint,
  onContinue,
}) => {
  return (
    <div className="relative overflow-hidden rounded-xl bg-linear-to-br from-secondary/10 via-card to-primary/5 border border-secondary/30 p-6 md:p-8 shadow-card">
      {/* Background */}
      <div className="absolute top-0 right-0 w-48 h-48 bg-secondary/10 rounded-full blur-3xl" />
      <div className="absolute bottom-0 left-0 w-32 h-32 bg-primary/10 rounded-full blur-2xl" />

      <div className="relative z-10">
        {/* Header */}
        <div className="flex items-center gap-2 mb-4">
          <div className="w-8 h-8 rounded-lg bg-secondary/20 flex items-center justify-center">
            <Zap className="w-4 h-4 text-secondary" />
          </div>
          <span className="text-sm font-medium text-secondary">
            Current Focus
          </span>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main */}
          <div className="lg:col-span-2">
            <h2 className="text-xl md:text-2xl font-heading font-bold text-text-foreground mb-4">
              {moduleName}
            </h2>

            {/* Progress */}
            <div className="mb-4">
              <div className="flex items-center justify-between mb-2">
                <span className="text-sm text-text-muted">Module Progress</span>
                <span className="text-sm font-semibold text-text-primary">
                  {progress}%
                </span>
              </div>

              <div className="relative h-2 rounded-full bg-bg-card/50 overflow-hidden">
                <div
                  className="absolute top-0 left-0 h-full rounded-full bg-linear-to-r from-secondary to-primary transition-all duration-500"
                  style={{ width: `${progress}%` }}
                />
              </div>
            </div>

            {/* Stats */}
            <div className="flex flex-wrap items-center gap-4 mb-6">
              <div className="flex items-center gap-2 text-sm text-text-muted">
                <BookOpen className="w-4 h-4" />
                <span>
                  {completedLessons} / {totalLessons} lessons
                </span>
              </div>

              <div className="flex items-center gap-2 text-sm text-text-muted">
                <Clock className="w-4 h-4" />
                <span>Last: {lastLesson}</span>
              </div>
            </div>

            {/* AI Insight */}
            {weakPoint && (
              <div className="p-4 rounded-lg bg-primary/5 border border-primary/20 mb-6">
                <p className="text-sm text-text-muted">
                  <span className="text-primary font-medium">AI Insight: </span>
                  Your current weak point is{" "}
                  <span className="text-text-foreground font-medium">
                    {weakPoint}
                  </span>
                  . Upcoming lessons will focus on strengthening this area.
                </p>
              </div>
            )}

            {/* Button */}
            <Button
              size="lg"
              onClick={onContinue}
              className="w-full sm:w-auto bg-linear-to-r from-primary to-secondary hover:from-primary-dark hover:to-primary text-primary-foreground font-semibold shadow-neon transition-all duration-300 hover:shadow-blue"
            >
              Continue Learning
              <ArrowRight className="w-5 h-5 ml-2" />
            </Button>
          </div>

          {/* Side UI */}
          <div className="hidden lg:flex items-center justify-center">
            <div className="relative w-40 h-40">
              <div className="absolute inset-0 rounded-full border-2 border-dashed border-text-accent animate-[spin_20s_linear_infinite]" />
              <div className="absolute inset-4 rounded-full border-2 border-dashed border-primary-dark animate-[spin_15s_linear_infinite_reverse]" />

              <div className="absolute inset-8 rounded-full bg-primary/20 flex items-center justify-center shadow-neon text-text-foreground">
                <span className="text-3xl font-heading font-bold primary-gradient">
                  {progress}%
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default CurrentFocusSection;
