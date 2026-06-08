"use client";

import {
  Sparkles,
  Lightbulb,
  Wand2,
  ListChecks,
  TrendingUp,
  AlertCircle,
} from "lucide-react";
import { Button } from "@/components/ui/button";

export default function AIMentorPanel() {
  return (
    <aside className="hidden xl:flex w-[340px] shrink-0 flex-col border-l border-border bg-card/60 backdrop-blur-xl">
      {/* Mentor header */}
      <div className="p-6 border-b border-border">
        <div className="flex items-center gap-3">
          <div className="relative">
            <div className="h-12 w-12 rounded-full gradient-cta flex items-center justify-center shadow-neon glow-pulse">
              <Sparkles className="h-5 w-5 text-foreground-text-foreground" />
            </div>
            <span className="absolute -bottom-0.5 -right-0.5 h-3 w-3 rounded-full bg-success border-2 border-card" />
          </div>
          <div>
            <p className="text-[11px] uppercase tracking-[0.18em] muted-text-foreground">
              AI Mentor
            </p>
            <h3 className="font-display font-semibold primary-gradient">
              Mentra
            </h3>
          </div>
        </div>

        <div className="mt-5 rounded-lg border border-primary/20 bg-surface/5 p-4">
          <p className="text-sm leading-relaxed text-foreground/90">
            You're doing great,{" "}
            <span className="text-foreground-light font-medium">Sara</span>.
            Your pace on gradient concepts is above average.
          </p>
          <div className="mt-3 flex items-center gap-1 text-xs muted-text-foreground">
            <span className="ai-dot h-1.5 w-1.5 rounded-full bg-surface" />
            <span className="ai-dot h-1.5 w-1.5 rounded-full bg-surface" />
            <span className="ai-dot h-1.5 w-1.5 rounded-full bg-surface" />
            <span className="ml-2">Mentra is observing</span>
          </div>
        </div>
      </div>

      {/* Insights */}
      <div className="p-6 border-b border-border space-y-4">
        <p className="text-[11px] uppercase tracking-[0.18em] muted-text-foreground">
          Real-time insights
        </p>

        <div>
          <div className="flex justify-between text-xs mb-2">
            <span className="muted-text-foreground">Understanding</span>
            <span className="text-success font-medium">Strong · 86%</span>
          </div>
          <div className="h-1.5 rounded-full bg-muted overflow-hidden">
            <div className="h-full w-[86%] bg-success rounded-full" />
          </div>
        </div>

        <div>
          <div className="flex justify-between text-xs mb-2">
            <span className="muted-text-foreground">Focus</span>
            <span className="text-secondary font-medium">Steady · 72%</span>
          </div>
          <div className="h-1.5 rounded-full bg-muted overflow-hidden">
            <div className="h-full w-[72%] bg-secondary rounded-full" />
          </div>
        </div>

        <div className="flex items-start gap-2.5 rounded-md border border-border bg-surface-elevated/40 p-3">
          <AlertCircle className="h-4 w-4 text-accent shrink-0 mt-0.5" />
          <p className="text-xs text-foreground/85 leading-relaxed">
            Consider revisiting{" "}
            <span className="text-accent">learning rate decay</span> from Lesson
            03 — it connects directly to this section.
          </p>
        </div>

        <div className="flex items-start gap-2.5 rounded-md border border-border bg-surface-elevated/40 p-3">
          <TrendingUp className="h-4 w-4 text-success shrink-0 mt-0.5" />
          <p className="text-xs text-foreground/85 leading-relaxed">
            You're <span className="text-success">12% ahead</span> of the
            average learner on this module.
          </p>
        </div>
      </div>

      {/* Quick actions */}
      <div className="p-6 space-y-2.5 flex-1">
        <p className="text-[11px] uppercase tracking-[0.18em] muted-text-foreground mb-3">
          Quick actions
        </p>

        <Button
          variant="outline"
          className="w-full justify-start gap-2.5 border-border hover:border-primary/40 hover:bg-surface/5 h-auto py-3"
        >
          <Wand2 className="h-4 w-4 text-foreground-light" />
          <span className="text-sm">Simplify this concept</span>
        </Button>

        <Button
          variant="outline"
          className="w-full justify-start gap-2.5 border-border hover:border-primary/40 hover:bg-surface/5 h-auto py-3"
        >
          <Lightbulb className="h-4 w-4 text-secondary" />
          <span className="text-sm">Give me an example</span>
        </Button>

        <Button
          variant="outline"
          className="w-full justify-start gap-2.5 border-border hover:border-primary/40 hover:bg-surface/5 h-auto py-3"
        >
          <ListChecks className="h-4 w-4 text-accent" />
          <span className="text-sm">Test me on this</span>
        </Button>
      </div>

      {/* Mini chat */}
      <div className="p-4 border-t border-border">
        <div className="flex items-center gap-2 rounded-md border border-border bg-surface-elevated/60 px-3 py-2.5 focus-within:border-primary/40 transition-fast">
          <Sparkles className="h-4 w-4 text-foreground-light shrink-0" />
          <input
            placeholder="Ask Mentra anything…"
            className="bg-transparent text-sm flex-1 outline-none placeholder:muted-text-foreground"
          />
          <kbd className="text-[10px] muted-text-foreground border border-border-strong rounded px-1.5 py-0.5">
            ⌘K
          </kbd>
        </div>
      </div>
    </aside>
  );
}
