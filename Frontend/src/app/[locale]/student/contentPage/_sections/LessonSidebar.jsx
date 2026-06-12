"use client";

import { Check, Lock, Play, Sparkles, Brain } from "lucide-react";
import { cn } from "@/lib/utils";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Link } from "@/lib/i18n/navigation";

export default function LessonSidebar({ open, lessons = [], setNumoflesson, stageProgressId }) {
  return (
    <aside
      className={cn(
        "fixed top-0 left-0 h-screen lg:sticky lg:top-0 lg:flex w-72 shrink-0 flex-col z-20 border-r border-border bg-card/60 backdrop-blur-xl transition-all duration-300",
        open ? "left-0" : "-left-72",
      )}
    >
      <Link href="/">
        <div className="px-6 py-5 border-b border-border">
          <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain dark:hidden" />
          <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain hidden dark:block" />
        </div>
      </Link>

      <nav className="flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-primary/30 px-3 py-4">
        <TooltipProvider delayDuration={200}>
          <ul className="space-y-1">
            {lessons.map((lesson, idx) => (
              <Tooltip key={lesson.id || lesson.lessonId || `lesson-${idx}`}>
                <TooltipTrigger asChild>
                  <button
                    className={cn(
                      "group w-full text-left flex items-start gap-3 px-3 py-2.5 rounded-md transition-fast border-b border-border my-6 cursor-pointer",
                      lesson.state === "current" &&
                        "bg-surface/10 border-primary/30 shadow-neon",
                      lesson.state === "completed" && "hover:bg-muted/40",
                      lesson.state === "locked" &&
                        "opacity-55 cursor-not-allowed",
                      lesson.isRemedial && "border-amber-500/35 bg-amber-500/5 hover:bg-amber-500/10"
                    )}
                    disabled={lesson.state === "locked"}
                    onClick={() => setNumoflesson(idx)}
                  >
                    {/*  دي الايقونات اللي جمب الكارت بتتغير بناء علي حالة الدرس */}
                    {/* <span
                        className={cn(
                          "mt-0.5 h-5 w-5 shrink-0 rounded-full flex items-center justify-center text-[10px] font-medium",
                          lesson.state === "completed" &&
                            "bg-success/20 text-success",
                          lesson.state === "current" &&
                            "gradient-cta text-foreground",
                          lesson.state === "locked" &&
                            "bg-bg-muted text-foreground-muted",
                        )}
                      >
                        {lesson.state === "completed" && (
                          <Check className="h-3 w-3" />
                        )}
                        {lesson.state === "current" && (
                          <Play className="h-3 w-3 fill-current" />
                        )}
                        {lesson.state === "locked" && (
                          <Lock className="h-3 w-3" />
                        )}
                      </span> */}
                    {/* دي النص اللي جنب الايقونات */}
                    <span className="min-w-0 flex-1 hover:underline">
                      <div className="flex items-center justify-between gap-1.5 my-2">
                        <span
                          className={cn(
                            "block text-[11px] tracking-wider muted-text-foreground",
                            lesson.state === "current" && "text-foreground-light",
                          )}
                        >
                          Lesson {String(idx + 1).padStart(2, "0")}
                        </span>
                        {lesson.isRemedial && (
                          <span className="inline-flex items-center gap-0.5 px-1.5 py-0.5 rounded bg-amber-500/15 border border-amber-500/35 text-[9px] font-extrabold text-amber-500 uppercase tracking-wide">
                            <Brain className="w-2.5 h-2.5" /> Remedial
                          </span>
                        )}
                      </div>

                      <span
                        className={cn(
                          "block text-sm leading-snug",
                          lesson.state === "current"
                            ? "text-foreground font-medium"
                            : "text-foreground/85",
                        )}
                      >
                        {lesson.title}
                      </span>
                    </span>
                  </button>
                </TooltipTrigger>

                {/* {(lesson.preview || lesson.state === "locked") && (
                  <TooltipContent
                    side="right"
                    className="max-w-[220px] bg-surface-dark/80 border-border"
                  >
                    <p className="text-xs text-foreground">
                      {lesson.state === "locked"
                        ? "🔒 Complete the previous lesson to unlock this one."
                        : lesson.preview}
                    </p>
                  </TooltipContent>
                )} */}
              </Tooltip>
            ))}
          </ul>
        </TooltipProvider>
      </nav>

      {/* Stage Quiz Button Container at the bottom of the sidebar */}
      <div className="p-4 border-t border-border bg-background/50 backdrop-blur-md">
        <div className="rounded-xl border border-primary/20 bg-linear-to-br from-primary/5 to-secondary/5 p-3 mb-3 text-center">
          <div className="flex items-center justify-center gap-1.5 mb-1">
            <Brain className="w-4 h-4 text-primary animate-pulse" />
            <span className="text-[11px] font-semibold uppercase tracking-[0.12em] text-foreground-light">Stage Progress</span>
          </div>
          <p className="text-[10px] text-foreground-muted leading-relaxed">
            Ready to test your knowledge? Take the stage quiz to unlock the next level.
          </p>
        </div>

        <Link
          href={`/student/quizPage?stageProgressId=${stageProgressId}`}
          className="block w-full"
        >
          <button className="w-full flex items-center justify-center gap-2 py-3 px-4 rounded-xl bg-primary text-foreground hover:bg-primary-dark font-semibold text-sm transition-all shadow-neon hover:shadow-neon-hover active:scale-[0.98] cursor-pointer">
            <Brain className="w-4 h-4 text-foreground" />
            <span>Start Stage Quiz</span>
          </button>
        </Link>
      </div>
    </aside>
  );
};
