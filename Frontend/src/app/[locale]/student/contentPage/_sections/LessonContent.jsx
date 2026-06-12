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
} from "lucide-react";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { useState } from "react";
import { cn } from "@/lib/utils";
import { Link } from "@/lib/i18n/navigation";

export default function LessonContent({ setOpenSidebar, video, article }) {
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
            <span>Module 02</span>
            <ChevronRight className="h-3 w-3 shrink-0" />
            <span className="text-foreground/80">{video?.title}</span>
          </div>
        </div>
      </div>

      <article className="main-container animate-fade-in-up">
        {/* Header */}
        <header className="mb-10">
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
                  unlock everything in{" "}
                  <span className="text-accent">
                    Module 03: Optimizers & Convergence
                  </span>
                  . Slow down on the visualization section.
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
                className="w-full h-[400px] rounded-lg"
                src={embedUrl}
                title="YouTube video player"
                allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
                referrerPolicy="strict-origin-when-cross-origin"
                allowFullScreen
              ></iframe>
            );
          })()}
          <h2 className="font-display text-2xl md:text-3xl font-semibold tracking-tight leading-[1.05] flex items-center gap-2">
            <BookOpenText className="h-8 w-8 text-foreground" /> Read the
            Article From Here
          </h2>

          <div>
            <Link
              href={article?.url || "#"}
              target="_blank"
              rel="noopener noreferrer"
              className="inline-flex items-center gap-2 px-6 py-3 rounded-lg bg-primary/15 border border-primary/40 text-foreground font-semibold hover:bg-primary hover:border-primary hover:text-foreground shadow-md transition-all duration-200 active:scale-[0.98]"
            >
              <BookOpenText className="h-5 w-5" />
              Read the Article
            </Link>
          </div>
        </section>

        {/* End-of-lesson */}
        <section className="mt-16 rounded-2xl border border-border bg-linear-to-br from-card to-surface-elevated/40 p-8">
          <div className="flex items-center gap-2 mb-2">
            <Sparkles className="h-4 w-4 text-foreground-light" />
            <p className="text-[11px] uppercase tracking-[0.18em] text-foreground-light">
              AI Summary
            </p>
          </div>

          <h2 className="font-display text-2xl font-semibold mb-4 tracking-tight">
            What you learned
          </h2>

          <ul className="space-y-2.5 text-sm text-foreground/85">
            <li className="flex gap-2.5">
              <CheckCircle2 className="h-4 w-4 text-success mt-0.5 shrink-0" />
              Function in Python is a block of organized, reusable code that is
              used to perform a single, related action.
            </li>
            <li className="flex gap-2.5">
              <CheckCircle2 className="h-4 w-4 text-success mt-0.5 shrink-0" />
              Function in Python is a block of organized, reusable code that is
              used to perform a single, related action.
            </li>
            <li className="flex gap-2.5">
              <CheckCircle2 className="h-4 w-4 text-success mt-0.5 shrink-0" />
              Function in Python is a block of organized, reusable code that is
              used to perform a single, related action.
            </li>
            <li className="flex gap-2.5">
              <CheckCircle2 className="h-4 w-4 text-success mt-0.5 shrink-0" />
              Function in Python is a block of organized, reusable code that is
              used to perform a single, related action.
            </li>
          </ul>

          <div className="mt-6 rounded-lg border border-success/25 bg-success/5 p-4">
            <p className="text-sm text-foreground/90">
              <span className="text-success font-medium">
                Mentra's verdict:
              </span>{" "}
              Strong understanding overall. Before moving on, briefly review{" "}
              <span className="text-accent">momentum</span> from Lesson 03 —
              it'll make Lesson 05 much smoother.
            </p>
          </div>

          <Link
            href="/student/quizPage"
            className="mt-7 flex items-center gap-3"
          >
            <Button className="group cursor-pointer hover:opacity-90 transition shadow-neon text-foreground bg-primary hover:bg-primary-dark gap-2 h-11 px-6">
              Start Quiz{" "}
              <ArrowRight className="h-4 w-4 group-hover:translate-x-1 transition-transform" />
            </Button>
          </Link>
        </section>

        <div className="h-12" />
      </article>
    </main>
  );
}
