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
        "bg-bg-card/50 backdrop-blur-sm",
        isPrimary && "border-primary/30 scale-[1.02]",
        isPrimary && "bg-primary/10",
      )}
    >
      {/* Primary badge */}
      {isPrimary && (
        <div className="absolute -top-3 left-1/2 -translate-x-1/2 px-4 py-1 rounded-full bg-primary text-text-foreground text-xs font-medium flex items-center gap-1">
          <Sparkles className="w-3 h-3" />
          Recommended
        </div>
      )}

      {/* Icon */}
      <div
        className={cn(
          "w-14 h-14 rounded-xl mb-6 flex items-center justify-center",
          "bg-linear-to-br",
          isPrimary
            ? "from-primary/20 to-secondary/20"
            : "from-muted to-bg-tertiary",
        )}
      >
        {icon}
      </div>

      {/* Content */}
      <h3 className="text-xl font-semibold text-text-foreground mb-3">
        {title}
      </h3>

      <p className="text-text-muted mb-6 leading-relaxed">{description}</p>

      {/* CTA Button */}
      <Button
        onClick={onClick}
        className={cn(
          "w-full group/btn transition-all duration-300 cursor-pointer",
          isPrimary
            ? "bg-primary hover:bg-primary-dark text-text-foreground glow-primary"
            : "bg-bg-muted hover:bg-bg-tertiary text-text-foreground border border-border",
        )}
        size="lg"
      >
        {ctaText}
        <ArrowRight className="w-4 h-4 ml-2 group-hover/btn:translate-x-1 transition-transform" />
      </Button>
    </div>
  );
}
