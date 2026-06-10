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
import { Link } from "@/lib/i18n/navigation";

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
  ACTIVE: {
    icon: Play,
    iconColor: "text-secondary",
    borderColor: "border-secondary/50",
    bgColor: "bg-secondary/10",
    glowClass: "pulse-current",
    label: "In Progress",
    buttonLabel: "Continue",
    buttonVariant: "default",
  },
  LOCKED: {
    icon: Lock,
    iconColor: "text-foreground-muted",
    borderColor: "border-border",
    bgColor: "bg-muted/10",
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

const ModuleCard = ({
  title,
  lessonCount,
  status,
  index,
  totalModules,
  aiTooltip,
  onClick,
  id,
}) => {
  console.log("ModuleCard status:", status);

  const config = statusConfig[status] || statusConfig["LOCKED"];
  const Icon = config.icon;
  const isDisabled = status === "LOCKED";

  return (
    <Link href={`/student/contentPage/${id}`}>
      <div
        className="relative flex items-start gap-4 animate-slide-in mb-4"
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
                "w-0.5 h-36 md:h-24 mt-2 rounded-full transition-colors duration-300",
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
          <div className="flex flex-col md:flex-row items-start justify-between gap-4">
            <div className="flex-1 min-w-0">
              {/* Badges */}
              <div className="flex items-center gap-2 mb-2">
                <span
                  className={cn(
                    "text-xs font-medium px-2 py-0.5 rounded-full border",
                    status === "completed" &&
                      "text-success bg-success/10 border-success/20",
                    status === "ACTIVE" &&
                      "text-secondary bg-secondary/10 border-secondary/20",
                    status === "LOCKED" &&
                      "text-foreground-muted bg-background/50 border border-border",
                    status === "needsReview" &&
                      "text-destructive bg-destructive/10 border-destructive/20",
                  )}
                >
                  {config.label}
                </span>
              </div>

              {/* Title */}
              <h3
                className={cn(
                  "font-heading font-semibold text-lg mb-2 transition-colors",
                  isDisabled
                    ? "text-foreground-muted"
                    : "text-foreground group-hover:text-foreground",
                )}
              >
                {title}
              </h3>

              {/* Lessons */}
              <p className="text-sm text-foreground-muted">
                {lessonCount} lessons
              </p>

              {/* AI Tooltip */}
              {aiTooltip && status === "locked" && (
                <div className="mt-3 p-3 rounded-lg bg-card/50 border border-border/50">
                  <p className="text-xs text-foreground-muted italic">
                    <span className="text-foreground font-medium not-italic">
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
                "shrink-0 text-foreground border-border hover:bg-surface/10",
                status === "ACTIVE" &&
                  "bg-linear-to-r from-primary to-secondary hover:from-primary-dark hover:to-primary text-background border-0 cursor-pointer",
              )}
            >
              {config.buttonLabel}
              {status === "ACTIVE" && <ChevronRight className="w-4 h-4 ml-1" />}
            </Button>
          </div>
        </div>
      </div>
    </Link>
  );
};

export default ModuleCard;
