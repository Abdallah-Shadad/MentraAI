"use client";
import { Brain, Sparkles, Target, Clock, Activity } from "lucide-react";

export default function Header() {
  return (
    <header className="flex flex-col gap-6 lg:flex-row lg:items-start lg:justify-between">
      <div className="flex items-start gap-4">
        <div className="relative">
          <div className="size-12 rounded-2xl bg-linear-to-br from-primary to-secondary grid place-items-center shadow-neon animate-glow">
            <Brain className="size-6 text-white" />
          </div>
          <span className="absolute -right-1 -top-1 size-3 rounded-full bg-success animate-pulse-soft" />
        </div>
        <div>
          <div className="flex items-center gap-2 text-xs uppercase tracking-[0.2em] text-foreground-muted">
            <Sparkles className="size-3.5 text-accent" />
            MentraAi · Understanding Check
          </div>
          <h1 className="mt-2 text-3xl sm:text-4xl font-semibold text-foreground">
            Module 2 Assessment
            <span className="text-foreground-muted font-normal">
              {" "}
              — JavaScript Basics
            </span>
          </h1>
          <div className="mt-3 flex flex-wrap items-center gap-2 text-sm">
            <Pill icon={<Target className="size-3.5" />}>5 questions</Pill>
            <Pill icon={<Clock className="size-3.5" />}>~12 min</Pill>
            <Pill icon={<Activity className="size-3.5" />}>
              Adaptive · Beginner+
            </Pill>
          </div>
        </div>
      </div>
    </header>
  );
}

function Pill({ icon, children }) {
  return (
    <span className="inline-flex items-center gap-1.5 rounded-full border border-border bg-card/60 px-3 py-1 text-xs text-foreground-secondary">
      {icon}
      {children}
    </span>
  );
}
