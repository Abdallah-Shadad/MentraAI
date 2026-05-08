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

const lessons = [
  {
    id: "1",
    title: "Introduction to Neural Networks",
    duration: "8 min",
    state: "completed",
  },
  {
    id: "2",
    title: "Perceptrons & Activation Functions",
    duration: "12 min",
    state: "completed",
  },
  {
    id: "3",
    title: "Backpropagation Intuition",
    duration: "15 min",
    state: "completed",
  },
  {
    id: "4",
    title: "Gradient Descent in Practice",
    duration: "18 min",
    state: "current",
    preview: "Keep learning step by step.",
  },
  {
    id: "5",
    title: "Optimizers: Adam, RMSProp, SGD",
    duration: "14 min",
    state: "locked",
    preview: "Complete the current lesson to unlock.",
  },
  {
    id: "6",
    title: "Regularization Techniques",
    duration: "11 min",
    state: "locked",
  },
  {
    id: "7",
    title: "Building Your First Model",
    duration: "22 min",
    state: "locked",
  },
  {
    id: "8",
    title: "Module Project: Handwritten Digits",
    duration: "30 min",
    state: "locked",
  },
];

export default function LessonSidebar({ open }) {
  return (
    <aside
      className={cn(
        "fixed top-0 left-0 min-h-screen lg:static lg:flex w-72 shrink-0 flex-col z-20 border-r border-border bg-bg-card/60 backdrop-blur-xl transition-all duration-300",
        open ? "left-0" : "-left-72",
      )}
    >
      <Link href="/">
        <div className="px-6 py-5 border-b border-border">
          <div className="flex items-center gap-2">
            <div className="h-8 w-8 rounded-lg gradient-cta flex items-center justify-center shadow-neon">
              <Sparkles className="h-4 w-4 text-primary-foreground" />
            </div>
            <span className="font-display font-semibold tracking-tight primary-gradient">
              MentraAI
            </span>
          </div>
        </div>
      </Link>

      <div className="px-6 py-5">
        <p className="text-[11px] uppercase tracking-[0.18em] text-muted-foreground">
          Track
        </p>
        <h2 className="mt-1 font-display text-base font-semibold">
          Deep Learning Foundations
        </h2>
        <p className="mt-3 text-[11px] uppercase tracking-[0.18em] text-muted-foreground">
          Module 02
        </p>
        <h3 className="mt-1 text-sm text-foreground/90">
          Training Neural Networks
        </h3>

        <div className="mt-4">
          <div className="flex justify-between text-xs text-text-muted mb-1.5">
            <span>Progress</span>
            <span className="text-foreground/80 font-medium">3 / 8</span>
          </div>
          <div className="h-1.5 rounded-full bg-primary/20 overflow-hidden">
            <div className="h-full w-[37%] gradient-cta  rounded-full" />
          </div>
        </div>
      </div>

      <nav className="flex-1 overflow-y-auto scrollbar-thin scrollbar-thumb-primary/30 h-[calc(100vh-12rem)] px-3 pb-6">
        <TooltipProvider delayDuration={200}>
          <ul className="space-y-1">
            {lessons.map((lesson, idx) => (
              <Tooltip key={lesson.id}>
                <TooltipTrigger asChild>
                  <li>
                    <button
                      className={cn(
                        "group w-full text-left flex items-start gap-3 px-3 py-2.5 rounded-md transition-fast border-b border-border my-6 cursor-pointer",
                        lesson.state === "current" &&
                          "bg-primary/10 border-primary/30 shadow-neon",
                        lesson.state === "completed" && "hover:bg-muted/40",
                        lesson.state === "locked" &&
                          "opacity-55 cursor-not-allowed",
                      )}
                      disabled={lesson.state === "locked"}
                    >
                      {/*  دي الايقونات اللي جمب الكارت بتتغير بناء علي حالة الدرس */}
                      <span
                        className={cn(
                          "mt-0.5 h-5 w-5 shrink-0 rounded-full flex items-center justify-center text-[10px] font-medium",
                          lesson.state === "completed" &&
                            "bg-success/20 text-success",
                          lesson.state === "current" &&
                            "gradient-cta text-text-primary",
                          lesson.state === "locked" &&
                            "bg-bg-muted text-text-muted",
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
                      </span>
                      {/* دي النص اللي جنب الايقونات */}
                      <span className="min-w-0 flex-1 hover:underline">
                        <span
                          className={cn(
                            "block text-[11px] tracking-wider text-muted-foreground",
                            lesson.state === "current" && "text-primary-light",
                          )}
                        >
                          Lesson {String(idx + 1).padStart(2, "0")}
                        </span>

                        <span
                          className={cn(
                            "block text-sm leading-snug truncate",
                            lesson.state === "current"
                              ? "text-foreground font-medium"
                              : "text-foreground/85",
                          )}
                        >
                          {lesson.title}
                        </span>

                        <span className="block text-[11px] text-muted-foreground mt-0.5">
                          {lesson.duration}
                        </span>
                      </span>
                    </button>
                  </li>
                </TooltipTrigger>

                {(lesson.preview || lesson.state === "locked") && (
                  <TooltipContent
                    side="right"
                    className="max-w-[220px] bg-primary-dark/80 border-border"
                  >
                    <p className="text-xs text-text-foreground">
                      {lesson.state === "locked"
                        ? "🔒 Complete the previous lesson to unlock this one."
                        : lesson.preview}
                    </p>
                  </TooltipContent>
                )}
              </Tooltip>
            ))}
          </ul>
        </TooltipProvider>
      </nav>
    </aside>
  );
}
