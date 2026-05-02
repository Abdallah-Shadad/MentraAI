"use client";

import {
  CheckCircle2,
  Lock,
  AlertCircle,
  Play,
  RotateCcw,
  ChevronRight,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { cn } from "@/lib/utils";

const statusConfig = {
  completed: {
    icon: CheckCircle2,
    iconColor: "text-success",
    borderColor: "border-success/30",
    bgColor: "bg-success/5",
    glowClass: "glow-success",
    label: "Completed",
    buttonLabel: "Review",
    buttonVariant: "outline",
  },
  current: {
    icon: Play,
    iconColor: "text-secondary",
    borderColor: "border-secondary/50",
    bgColor: "bg-secondary/10",
    glowClass: "pulse-current",
    label: "In Progress",
    buttonLabel: "Continue",
    buttonVariant: "default",
  },
  locked: {
    icon: Lock,
    iconColor: "text-text-muted",
    borderColor: "border-border",
    bgColor: "bg-text-muted/10",
    glowClass: "",
    label: "Locked",
    buttonLabel: "Locked",
    buttonVariant: "secondary",
  },
  needsReview: {
    icon: AlertCircle,
    iconColor: "text-destructive",
    borderColor: "border-destructive/30",
    bgColor: "bg-destructive/5",
    glowClass: "bg-destructive/5",
    label: "Needs Review",
    buttonLabel: "Review",
    buttonVariant: "outline",
  },
};

const difficultyColors = {
  Beginner: "text-success bg-success/10 border-success/20",
  Intermediate: "text-secondary bg-secondary/10 border-secondary/20",
  Advanced: "text-primary bg-primary/10 border-primary/20",
};

const ModuleCard = ({
  title,
  lessonCount,
  difficulty,
  status,
  index,
  totalModules,
  aiTooltip,
  onClick,
}) => {
  const config = statusConfig[status] || statusConfig["locked"];
  const Icon = config.icon;
  const isDisabled = status === "locked";

  return (
    <div
      className="relative flex items-start gap-4 animate-slide-in"
      style={{ animationDelay: `${index * 100}ms` }}
    >
      {/* Timeline */}
      <div className="flex flex-col items-center">
        {/* Node */}
        <div
          className={cn(
            "relative z-10 w-12 h-12 rounded-full flex items-center justify-center border-2 transition-all duration-300",
            config.borderColor,
            config.bgColor,
            config.glowClass,
          )}
        >
          <Icon className={cn("w-5 h-5", config.iconColor)} />
        </div>

        {/* Line */}
        {index < totalModules - 1 && (
          <div
            className={cn(
              "w-0.5 h-24 mt-2 rounded-full transition-colors duration-300",
              status === "completed" ? "bg-success/50" : "bg-border",
            )}
          />
        )}
      </div>

      {/* Card */}
      <div
        className={cn(
          "flex-1 p-5 rounded-xl border transition-all duration-300 group",
          config.bgColor,
          config.borderColor,
          !isDisabled &&
            "hover:border-primary/30 hover:shadow-card cursor-pointer",
        )}
        onClick={!isDisabled ? onClick : undefined}
      >
        <div className="flex items-start justify-between gap-4">
          <div className="flex-1 min-w-0">
            {/* Badges */}
            <div className="flex items-center gap-2 mb-2">
              <span
                className={cn(
                  "text-xs font-medium px-2 py-0.5 rounded-full border",
                  status === "completed" &&
                    "text-success bg-success/10 border-success/20",
                  status === "current" &&
                    "text-secondary bg-secondary/10 border-secondary/20",
                  status === "locked" &&
                    "text-muted-foreground bg-muted/50 border-border",
                  status === "needsReview" &&
                    "text-warning bg-warning/10 border-warning/20",
                )}
              >
                {config.label}
              </span>

              <span
                className={cn(
                  "text-xs font-medium px-2 py-0.5 rounded-full border",
                  difficultyColors[difficulty],
                )}
              >
                {difficulty}
              </span>
            </div>

            {/* Title */}
            <h3
              className={cn(
                "font-heading font-semibold text-lg mb-2 transition-colors",
                isDisabled
                  ? "text-text-muted"
                  : "text-text-foreground group-hover:text-primary",
              )}
            >
              {title}
            </h3>

            {/* Lessons */}
            <p className="text-sm text-text-muted">{lessonCount} lessons</p>

            {/* AI Tooltip */}
            {aiTooltip && status === "locked" && (
              <div className="mt-3 p-3 rounded-lg bg-bg-card/50 border border-border/50">
                <p className="text-xs text-text-muted italic">
                  <span className="text-text-primary font-medium not-italic">
                    AI:{" "}
                  </span>
                  {aiTooltip}
                </p>
              </div>
            )}
          </div>

          {/* Button */}
          <Button
            variant={config.buttonVariant}
            size="sm"
            disabled={isDisabled}
            className={cn(
              "shrink-0 text-text-foreground border-border hover:bg-primary/10",
              status === "current" &&
                "bg-linear-to-r from-primary to-secondary hover:from-primary-dark hover:to-primary text-text-foreground border-0",
            )}
          >
            {status === "needsReview" && <RotateCcw className="w-4 h-4 mr-1" />}
            {config.buttonLabel}
            {status === "current" && <ChevronRight className="w-4 h-4 ml-1" />}
          </Button>
        </div>
      </div>
    </div>
  );
};

export default ModuleCard;
