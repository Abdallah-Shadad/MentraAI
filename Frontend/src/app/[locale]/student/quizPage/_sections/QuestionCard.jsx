"use client";
import { Zap, CheckCircle2, HelpCircle, Loader2, Lightbulb } from "lucide-react";
import { motion } from "framer-motion";

export default function QuestionCard({ q, index, total, selected, onSelect, unlockedHints = [], hintLoading, onUnlockHint }) {
  if (!q) return null;

  const choices = q.choices || [];
  const text = q.text || "";
  const concept = q.concept || "Core Concept";

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
          {concept}
        </span>
      </div>

      <h2 className="mt-4 text-2xl sm:text-3xl leading-snug text-foreground font-semibold">
        {text}
      </h2>

      <div className="mt-7 grid gap-3">
        {choices.map((opt, i) => {
          const active = selected === opt.label;
          return (
            <motion.button
              key={opt.label}
              onClick={() => onSelect(opt.label)}
              whileHover={{ scale: 1.005 }}
              whileTap={{ scale: 0.995 }}
              className={`group relative text-left rounded-2xl border px-5 py-4 transition-all duration-150 flex items-center gap-4 cursor-pointer ${
                active
                  ? "border-primary bg-primary/5 ring-1 ring-primary/20"
                  : "border-border bg-surface-elevated/40 hover:border-primary/50 hover:bg-surface-elevated/80"
              }`}
            >
              <span
                className={
                  `size-8 shrink-0 rounded-lg grid place-items-center text-sm font-bold transition-colors ` +
                  (active
                    ? "bg-linear-to-r from-primary to-secondary text-white"
                    : "bg-card text-foreground-muted group-hover:text-foreground")
                }
              >
                {opt.label}
              </span>
              <span
                className={`text-[15px] leading-relaxed text-foreground ${active ? "font-semibold" : ""}`}
              >
                {opt.text}
              </span>
              {active && (
                <CheckCircle2 className="ml-auto size-5 text-primary animate-scale-in shrink-0" />
              )}
            </motion.button>
          );
        })}
      </div>

      {/* Interactive Hints Section */}
      <div className="mt-8 pt-6 border-t border-border/60">
        <div className="flex flex-col gap-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2 text-xs font-semibold text-foreground-muted uppercase tracking-wider">
              <Lightbulb className="w-4 h-4 text-amber-500 animate-pulse" />
              <span>Stuck? AI Mentor Hints</span>
            </div>
            {unlockedHints.length < 3 && (
              <button
                type="button"
                onClick={onUnlockHint}
                disabled={hintLoading}
                className="text-xs font-bold text-primary hover:text-primary/85 hover:underline flex items-center gap-1 cursor-pointer disabled:opacity-50 disabled:no-underline"
              >
                {hintLoading ? (
                  <>
                    <Loader2 className="w-3 h-3 animate-spin" />
                    <span>Analyzing...</span>
                  </>
                ) : (
                  <span>Request Hint {unlockedHints.length + 1}/3</span>
                )}
              </button>
            )}
          </div>

          {unlockedHints.length > 0 ? (
            <div className="space-y-3">
              {unlockedHints.map((hint, idx) => (
                <motion.div
                  key={idx}
                  initial={{ opacity: 0, y: 5 }}
                  animate={{ opacity: 1, y: 0 }}
                  className="p-3.5 rounded-xl border border-amber-500/10 bg-amber-500/5 text-xs text-foreground/90 leading-relaxed flex items-start gap-2.5"
                >
                  <span className="inline-flex items-center justify-center shrink-0 w-5 h-5 rounded-md bg-amber-500/20 text-[10px] text-amber-500 font-extrabold uppercase">
                    L{idx + 1}
                  </span>
                  <span>{hint}</span>
                </motion.div>
              ))}
            </div>
          ) : (
            <p className="text-[11px] text-foreground-muted leading-relaxed">
              Unlock progressive tips to guide your reasoning without revealing the final answer directly.
            </p>
          )}
        </div>
      </div>
    </article>
  );
}
