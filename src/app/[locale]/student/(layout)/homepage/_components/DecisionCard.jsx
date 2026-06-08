"use client";

import { cn } from "@/lib/utils";
import { Button } from "@/components/ui/button";
import { Sparkles, ArrowRight } from "lucide-react";

export default function DecisionCard({
  title,
  description,
  ctaText,
  isPrimary = false,
  icon,
  onClick,
}) {
  return (
    <div
      className={cn(
        "relative group rounded-2xl p-8 transition-all duration-300",
        "border border-border hover:border-primary/50",
        "bg-card/50 backdrop-blur-sm",
        isPrimary && "border-primary/30 scale-[1.02]",
        isPrimary && "bg-surface/10",
      )}
    >
      {/* Primary badge */}
      {isPrimary && (
        <div className="absolute -top-3 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full bg-primary text-muted text-xs font-medium flex items-center gap-1">
          <Sparkles className="w-3 h-3" />
          Recommended
        </div>
      )}

      {/* Icon */}
      <div
        className={cn(
          "w-14 h-14 rounded-xl mb-6 flex items-center justify-center",
          isPrimary ? "bg-primary/25" : "bg-muted",
        )}
      >
        {icon}
      </div>

      {/* Content */}
      <h3 className="text-xl font-semibold text-foreground mb-3">{title}</h3>

      <p className="text-foreground-muted mb-6 leading-relaxed">
        {description}
      </p>

      {/* CTA Button */}
      <Button
        onClick={onClick}
        className={cn(
          "w-full group/btn transition-all duration-300 cursor-pointer",
          isPrimary
            ? "bg-primary/25 hover:bg-primary text-foreground hover:text-muted hover:dark:text-foreground"
            : "bg-muted hover:bg-primary text-foreground border border-border hover:text-muted hover:dark:text-foreground",
        )}
        size="lg"
      >
        {ctaText}
      </Button>
    </div>
  );
}
