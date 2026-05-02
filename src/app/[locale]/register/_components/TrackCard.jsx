"use client";

import { cn } from "@/lib/utils";
import { ArrowRight, Clock, Briefcase } from "lucide-react";

export const TrackCard = ({
  name,
  icon,
  level,
  duration,
  careers,
  learnPoints,
  onClick,
}) => {
  return (
    <div
      onClick={onClick}
      className={cn(
        "group relative rounded-xl p-6 cursor-pointer",
        "bg-bg-card border border-border",
        "transition-all duration-300",
        "hover:border-primary/50 hover:glow-primary hover:-translate-y-1",
      )}
    >
      {/* Icon */}
      <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary/20 to-secondary/10 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
        {icon}
      </div>

      {/* Track Name */}
      <h4 className="text-lg font-semibold text-text-foreground mb-2">
        {name}
      </h4>

      {/* Meta Info */}
      <div className="flex items-center gap-4 mb-4 text-sm text-text-muted">
        <span className="px-2 py-1 rounded bg-bg-tertiary">{level}</span>

        <span className="flex items-center gap-1">
          <Clock className="w-3.5 h-3.5" />
          {duration}
        </span>
      </div>

      {/* Career Badge */}
      <div className="flex flex-wrap gap-2 mb-4">
        {careers.slice(0, 2).map((career, idx) => (
          <span
            key={idx}
            className="px-2 py-1 text-xs rounded-full bg-secondary/10 text-secondary border border-secondary/20 flex items-center gap-1"
          >
            <Briefcase className="w-3 h-3" />
            {career}
          </span>
        ))}
      </div>

      {/* Hover Preview */}
      <div className="overflow-hidden max-h-0 group-hover:max-h-40 transition-all duration-300">
        {learnPoints && learnPoints.length > 0 && (
          <div className="pt-4 border-t border-border">
            <p className="text-xs text-text-accent mb-2">What you'll learn:</p>

            <ul className="space-y-1">
              {learnPoints.slice(0, 3).map((point, idx) => (
                <li
                  key={idx}
                  className="text-xs text-text-muted flex items-center gap-2"
                >
                  <span className="w-1 h-1 rounded-full bg-primary" />
                  {point}
                </li>
              ))}
            </ul>
          </div>
        )}
      </div>

      {/* Arrow indicator */}
      <div className="absolute bottom-4 right-4 opacity-0 group-hover:opacity-100 transition-opacity">
        <ArrowRight className="w-5 h-5 text-primary" />
      </div>
    </div>
  );
};
