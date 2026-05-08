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

export default function LessonContent({ setOpenSidebar }) {
  const [breakpointAnswer, setBreakpointAnswer] = useState(null);

  return (
    <main className="flex-1 overflow-y-auto">
      {/* Top bar */}
      <div className="sticky top-0 z-10 backdrop-blur-xl bg-background/80 border-b border-border mb-4">
        <div className="max-w-3xl mx-auto px-4 md:px-8 py-3 flex items-center justify-between text-xs text-muted-foreground">
          <div className="flex items-center gap-2">
            <Link href="/" className="hover:underline">
              Home
            </Link>{" "}
            <ChevronRight className="h-3 w-3 shrink-0" />
            <Link href="/student/homepage" className="hover:underline">
              Roadmap
            </Link>{" "}
            <ChevronRight className="h-3 w-3 shrink-0" />
            <span>Module 02</span>
            <ChevronRight className="h-3 w-3 shrink-0" />
            <span className="text-foreground/80">
              Lesson 04 · Gradient Descent in Practice
            </span>
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
                className="border-primary/30 bg-primary/5 text-primary-light font-normal"
              >
                Core Concept
              </Badge>
              <Badge
                variant="outline"
                className="border-border text-muted-foreground font-normal gap-1.5"
              >
                <Clock className="h-3 w-3" /> 18 min
              </Badge>
            </div>
            <div
              className="flex items-center gap-2 lg:hidden"
              onClick={() => setOpenSidebar(true)}
            >
              <Menu className="h-6 w-6 text-muted-foreground cursor-pointer" />
            </div>
          </div>

          <h1 className="font-display text-4xl md:text-5xl font-semibold tracking-tight leading-[1.05] flex items-center gap-2">
            <Tag className="h-8 w-8 text-primary" /> Function in Python
          </h1>

          <div className="mt-6 rounded-lg border border-primary/20 bg-linear-to-br from-primary/8 to-secondary/5 p-5">
            <div className="flex items-start gap-3">
              <div className="h-8 w-8 shrink-0 rounded-md gradient-ai flex items-center justify-center">
                <Sparkles className="h-4 w-4 text-primary-foreground" />
              </div>
              <div>
                <p className="text-[11px] uppercase tracking-[0.16em] text-primary-light mb-1.5">
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
          <iframe
            className="w-full h-[400px] rounded-lg"
            src="https://www.youtube.com/embed/Izwd_n-Ufqo?si=NLUFhYHbI1eVbAVq"
            title="YouTube video player"
            allow="accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share"
            referrerPolicy="strict-origin-when-cross-origin"
            allowFullScreen
          ></iframe>
          <h2 className="font-display text-2xl md:text-3xl font-semibold tracking-tight leading-[1.05] flex items-center gap-2">
            <BookOpenText className="h-8 w-8 text-primary" /> Read the
            documentation
          </h2>
          <iframe
            className="w-full h-[550px] rounded-lg"
            src="https://python-adv-web-apps.readthedocs.io/en/latest/functions.html"
            title="Python Functions"
          ></iframe>

          <div>
            <p className="text-sm md:text-base italic text-foreground/90 leading-relaxed mb-4">
              if the documentation is not loading here , please click the button
              below to read the documentation:
            </p>
            <Link
              href="https://python-adv-web-apps.readthedocs.io/en/latest/functions.html"
              target="_blank"
              rel="noopener noreferrer"
              className="bg-primary text-primary-foreground px-4 py-2 rounded-lg"
            >
              Read the documentation
            </Link>
          </div>
        </section>

        {/* End-of-lesson */}
        <section className="mt-16 rounded-2xl border border-border bg-linear-to-br from-bg-card to-bg-tertiary/40 p-8">
          <div className="flex items-center gap-2 mb-2">
            <Sparkles className="h-4 w-4 text-primary-light" />
            <p className="text-[11px] uppercase tracking-[0.18em] text-primary-light">
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
            <Button className="group cursor-pointer hover:opacity-90 transition shadow-neon text-primary-foreground gap-2 h-11 px-6">
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
