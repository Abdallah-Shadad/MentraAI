"use client";

import { Check, Lock, Play, Sparkles } from "lucide-react";
import { cn } from "@/lib/utils";
import {
  Tooltip,
  TooltipContent,
  TooltipProvider,
  TooltipTrigger,
} from "@/components/ui/tooltip";
import { Link } from "@/lib/i18n/navigation";

export default function LessonSidebar({ open, lessons = [], setNumoflesson }) {
  return (
    <aside
      className={cn(
        "fixed top-0 left-0 min-h-screen lg:static lg:flex w-72 shrink-0 flex-col z-20 border-r border-border bg-card/60 backdrop-blur-xl transition-all duration-300",
        open ? "left-0" : "-left-72",
      )}
    >
      <Link href="/">
        <div className="px-6 py-5 border-b border-border">
          <div className="flex items-center gap-2">
            <div className="h-8 w-8 rounded-lg gradient-cta flex items-center justify-center shadow-neon">
              <Sparkles className="h-4 w-4 text-foreground-text-foreground" />
            </div>
            <span className="font-display font-semibold tracking-tight primary-gradient">
              MentraAI
            </span>
          </div>
        </div>
      </Link>

      <nav className="flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-primary/30 h-[calc(100vh-12rem)] px-3 pb-6">
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
                      <span
                        className={cn(
                          "block text-[11px] tracking-wider muted-text-foreground my-2",
                          lesson.state === "current" && "text-foreground-light",
                        )}
                      >
                        Lesson {String(idx + 1).padStart(2, "0")}
                      </span>

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
    </aside>
  );
}
