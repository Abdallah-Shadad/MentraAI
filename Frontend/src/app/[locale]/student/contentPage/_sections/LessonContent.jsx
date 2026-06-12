"use client";

import {
  Clock,
  Gauge,
  Sparkles,
  ChevronRight,
  CheckCircle2,
  ArrowRight,
  BookOpenText,
  Tag,
  Menu,
  ExternalLink,
  Brain,
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { Link } from "@/lib/i18n/navigation";

export default function LessonContent({ setOpenSidebar, video, article, stageProgressId, isLastLesson, onNextLesson }) {
  console.log("video", video);
  console.log("article", article);
  const [breakpointAnswer, setBreakpointAnswer] = useState(null);

  return (
    <main className="flex-1 overflow-y-auto">
      {/* Top bar */}
      <div className="sticky top-0 z-10 backdrop-blur-xl bg-background/80 border-b border-border mb-4">
        <div className="max-w-3xl mx-auto px-4 md:px-8 py-3 flex items-center justify-between text-xs muted-text-foreground">
          <div className="flex items-center gap-2">
            <Link href="/student" className="hover:underline">
              Home
            </Link>{" "}
            <ChevronRight className="h-3 w-3 shrink-0" />
            <Link href="/student/roadmap" className="hover:underline">
              Roadmap
            </Link>{" "}
            <ChevronRight className="h-3 w-3 shrink-0" />
            <span>Module Stage</span>
            <ChevronRight className="h-3 w-3 shrink-0" />
            <span className="text-foreground/80">{video?.title}</span>
          </div>
        </div>
      </div>

      <article className="main-container animate-fade-in-up">
        {/* Header */}
        <header className="mb-10">
          {video?.isRemedial && (
            <div className="mb-6 rounded-xl border border-amber-500/35 bg-amber-500/5 p-4 flex items-start gap-3.5">
              <div className="w-10 h-10 rounded-lg bg-amber-500/10 flex items-center justify-center shrink-0 text-amber-500">
                <Brain className="h-5 w-5" />
              </div>
              <div>
                <h4 className="text-sm font-bold text-amber-500 mb-0.5 uppercase tracking-wide">
                  AI Remediation Active
                </h4>
                <p className="text-xs text-foreground-light leading-relaxed">
                  This resource was dynamically injected by your AI Mentor to address conceptual gaps identified in your recent quiz attempt. Re-read and review this module carefully before retaking the assessment.
                </p>
              </div>
            </div>
          )}
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-2 mb-4">
              <Badge
                variant="outline"
                className="border-primary/30 bg-surface/5 text-foreground-light font-normal"
              >
                Core Concept
              </Badge>
              <Badge
                variant="outline"
                className="border-border muted-text-foreground font-normal gap-1.5"
              >
                <Clock className="h-3 w-3" /> 18 min
              </Badge>
            </div>
            <div
              className="flex items-center gap-2 lg:hidden"
              onClick={() => setOpenSidebar(true)}
            >
              <Menu className="h-6 w-6 muted-text-foreground cursor-pointer" />
            </div>
          </div>

          <h1 className="font-display text-4xl md:text-5xl font-semibold tracking-tight leading-[1.05] flex items-center gap-2">
            <Tag className="h-8 w-8 text-foreground" /> {video?.title}
          </h1>

          <div className="mt-6 rounded-lg border border-primary/20 bg-linear-to-br from-primary/8 to-secondary/5 p-5">
            <div className="flex items-start gap-3">
              <div className="h-8 w-8 shrink-0 rounded-md gradient-ai flex items-center justify-center">
                <Sparkles className="h-4 w-4 text-foreground-text-foreground" />
              </div>
              <div>
                <p className="text-[11px] uppercase tracking-[0.16em] text-foreground-light mb-1.5">
                  AI Insight
                </p>
                <p className="text-sm text-foreground/90 leading-relaxed">
                  This lesson is foundational — the intuitions you build here
                  unlock everything in the following stages. Slow down on the reading and visualization section.
                </p>
              </div>
            </div>
          </div>
        </header>

        {/* Lesson Content */}
        <section className="flex flex-col gap-12">
          {(() => {
            const getEmbedUrl = (url) => {
              if (!url) return null;
              if (url.includes("v=")) {
                const parts = url.split("v=");
                const id = parts[1]?.split("&")[0];
                return id ? `https://www.youtube.com/embed/${id}` : null;
              }
              if (url.includes("youtu.be/")) {
                const parts = url.split("youtu.be/");
                const id = parts[1]?.split("?")[0];
                return id ? `https://www.youtube.com/embed/${id}` : null;
              }
              if (url.includes("embed/")) {
                return url;
              }
              return null;
            };
            const embedUrl = getEmbedUrl(video?.url);
            if (!embedUrl) return null;
            return (
              <iframe
                className="w-full h-[400px] rounded-lg border border-border"
                src={embedUrl}
                title="YouTube video player"
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                referrerPolicy="strict-origin-when-cross-origin"
                allowFullScreen
              ></iframe>
            );
          })()}
          <div className="rounded-2xl border border-amber-500/30 bg-gradient-to-br from-amber-500/5 to-orange-500/5 p-6 shadow-sm hover:border-amber-500/60 hover:shadow-[0_0_30px_rgba(245,158,11,0.1)] transition-all duration-300">
            <div className="flex flex-col md:flex-row md:items-center justify-between gap-4 mb-4">
              <div className="flex items-center gap-2.5">
                <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center shrink-0">
                  <ExternalLink className="h-5 w-5 text-amber-500" />
                </div>
                <div>
                  <Badge className="bg-amber-500/10 text-amber-500 dark:text-amber-400 border border-amber-500/20 text-[10px] tracking-wider uppercase font-semibold px-2 py-0.5 rounded-md">
                    External Article
                  </Badge>
                  <h3 className="text-lg font-bold mt-1 text-foreground">
                    {article?.title || "Recommended Reading Resource"}
                  </h3>
                </div>
              </div>
            </div>
            
            <p className="text-sm text-foreground/80 leading-relaxed mb-6">
              This high-impact resource opens in a secure external tab. Absorb the knowledge, study the core concepts, and return to this workspace ready to crush your stage quiz.
            </p>

            <a
              href={article?.url || "#"}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center justify-center gap-2 px-6 py-3 rounded-lg bg-amber-500/10 border border-amber-500/30 text-foreground font-semibold hover:bg-amber-500 hover:text-white dark:hover:text-black hover:border-amber-500 shadow-xs transition-all duration-250 active:scale-[0.98] cursor-pointer"
            >
              <BookOpenText className="h-5 w-5" />
              <span>Read the Article</span>
              <ExternalLink className="h-4 w-4 opacity-75" />
            </a>
          </div>
        </section>

        {/* Navigation / Next Lesson vs Stage Quiz */}
        {!isLastLesson ? (
          <div className="mt-12 flex justify-end">
            <Button
              onClick={onNextLesson}
              className="group cursor-pointer hover:opacity-90 transition text-foreground bg-primary hover:bg-primary-dark gap-2 h-11 px-6 font-semibold"
            >
              <span>Next Lesson</span>
              <ArrowRight className="h-4 w-4 group-hover:translate-x-1 transition-transform animate-pulse" />
            </Button>
          </div>
        ) : (
          /* End-of-stage Quiz Section */
          <section className="mt-16 rounded-2xl border border-primary/20 bg-linear-to-br from-primary/5 to-secondary/5 p-8 animate-fade-in-up">
            <div className="flex items-center gap-2 mb-2">
              <Sparkles className="h-4 w-4 text-primary animate-pulse" />
              <p className="text-[11px] uppercase tracking-[0.18em] text-primary font-semibold">
                Stage Completed!
              </p>
            </div>

            <h2 className="font-display text-2xl font-semibold mb-3 tracking-tight">
              Congratulations on completing all lessons!
            </h2>

            <p className="text-sm text-foreground/80 mb-6 leading-relaxed">
              You have successfully studied all core concepts and resource articles for this stage. To officially complete this module and unlock the next stage of your roadmap, please take the **Stage Assessment**.
            </p>

            <div className="rounded-xl border border-primary/30 bg-primary/5 p-5 flex items-start gap-3.5 mb-6">
              <div className="w-10 h-10 rounded-lg bg-primary/10 flex items-center justify-center shrink-0">
                <Brain className="h-5 w-5 text-primary animate-pulse" />
              </div>
              <div>
                <h4 className="text-sm font-semibold text-foreground mb-1">
                  Ready to start the assessment?
                </h4>
                <p className="text-xs text-foreground-muted leading-relaxed">
                  Use the <span className="text-primary font-semibold">Start Stage Quiz</span> button located at the bottom of the left sidebar to begin.
                </p>
              </div>
            </div>

          </section>
        )}

        <div className="h-12" />
      </article>
    </main>
  );
}
