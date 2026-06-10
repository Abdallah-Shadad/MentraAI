"use client";
import { Zap, CheckCircle2 } from "lucide-react";
export default function QuestionCard({ q, index, total, selected, onSelect }) {
  return (
    <article
      key={q.id}
      className="relative rounded-3xl border border-border bg-card/70 shadow-card p-7 sm:p-9 animate-fade-in"
    >
      <div className="absolute inset-x-0 -top-px h-px bg-linear-to-r from-transparent via-primary/60 to-transparent" />

      <div className="flex items-center justify-between">
        <div className="text-xs uppercase tracking-[0.2em] text-foreground-muted">
          Question {index + 1} of {total}
        </div>
        <span className="inline-flex items-center gap-1.5 rounded-full bg-surface/10 border border-primary/30 px-2.5 py-1 text-[11px] text-foreground-accent">
          <Zap className="size-3" />
          {q.concept}
        </span>
      </div>

      <h2 className="mt-4 text-2xl sm:text-3xl leading-snug text-foreground font-semibold">
        {q.prompt}
      </h2>

      <div className="mt-7 grid gap-3">
        {q.options.map((opt, i) => {
          const active = selected === opt.id;
          return (
            <button
              key={opt.id}
              onClick={() => onSelect(opt.id)}
              className={`group relative text-left rounded-2xl border px-5 py-4 transition-all duration-200 flex items-center gap-4 ${
                active
                  ? "border-primary bg-surface/10 ring-neon"
                  : "border-border bg-surface-elevated/40 hover:border-border-strong hover:bg-surface-elevated"
              }`}
            >
              <span
                className={
                  `size-8 shrink-0 rounded-lg grid place-items-center text-sm font-medium transition-colors ` +
                  (active
                    ? "bg-linear-to-r from-primary to-secondary text-white"
                    : "bg-card text-foreground-muted group-hover:text-foreground")
                }
              >
                {String.fromCharCode(65 + i)}
              </span>
              <span
                className={`text-[15px] leading-relaxed text-foreground ${active ? "font-semibold" : ""}`}
              >
                {opt.label}
              </span>
              {active && (
                <CheckCircle2 className="ml-auto size-5 text-primary animate-scale-in shrink-0" />
              )}
            </button>
          );
        })}
      </div>
    </article>
  );
}
