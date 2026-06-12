"use client";
import { useState, useEffect } from "react";
import { Link } from "@/lib/i18n/navigation";
import { useRouter } from "@/lib/i18n/navigation";
import { Sparkles, RefreshCw, ArrowRight, Brain, Check, X, Award, ExternalLink } from "lucide-react";
import { motion, AnimatePresence } from "framer-motion";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogDescription } from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";

export default function QuizResults({ submitResult, onReset, stageProgressId }) {
  const router = useRouter();
  const data = submitResult?.data || submitResult || {};
  const score = Math.round(data.score || 0);
  const correct = data.correctAnswers || 0;
  const total = data.totalQuestions || 0;
  const isPassed = score >= 70;
  const nextStage = data.nextStage;
  const roadmapAdapted = data.roadmapAdapted || false;

  // Show adaptation modal if roadmap was adapted on fail
  const [showAdaptation, setShowAdaptation] = useState(false);

  useEffect(() => {
    if (roadmapAdapted) {
      setShowAdaptation(true);
    }
  }, [roadmapAdapted]);

  // Score Ring Math
  const circumference = 2 * Math.PI * 45;
  const strokeDashoffset = circumference - (score / 100) * circumference;

  return (
    <div className="mt-8 grid grid-cols-1 lg:grid-cols-[1.2fr_1fr] gap-8 animate-fade-in text-foreground">
      {/* Left Panel: Analytics & Metrics */}
      <section className="rounded-3xl border border-border bg-card/85 p-7 sm:p-9 shadow-lg relative overflow-hidden">
        <div className="absolute top-0 right-0 w-32 h-32 bg-primary/5 rounded-full blur-3xl pointer-events-none" />
        
        <div className="flex items-center gap-2 text-xs uppercase tracking-[0.2em] text-primary font-semibold mb-6">
          <Sparkles className="size-4" /> Quiz Analysis
        </div>

        <div className="flex flex-col sm:flex-row items-center gap-8 mb-8">
          {/* Circular Progress Ring */}
          <div className="relative size-32 shrink-0">
            <svg className="size-32 -rotate-90" viewBox="0 0 100 100">
              <circle
                cx="50"
                cy="50"
                r="45"
                stroke="var(--border)"
                strokeWidth="7"
                fill="none"
                className="opacity-50"
              />
              <motion.circle
                cx="50"
                cy="50"
                r="45"
                stroke={isPassed ? "var(--success)" : "var(--destructive)"}
                strokeWidth="7"
                strokeLinecap="round"
                fill="none"
                strokeDasharray={circumference}
                initial={{ strokeDashoffset: circumference }}
                animate={{ strokeDashoffset }}
                transition={{ duration: 1, ease: "easeOut" }}
              />
            </svg>
            <div className="absolute inset-0 flex flex-col items-center justify-center">
              <span className="text-3xl font-extrabold">{score}%</span>
              <span className="text-[10px] text-foreground-muted uppercase tracking-wider font-semibold">
                Score
              </span>
            </div>
          </div>

          <div className="text-center sm:text-left">
            <div className="mb-2">
              {isPassed ? (
                <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-success/15 border border-success/30 text-success text-xs font-bold uppercase tracking-wider">
                  <Check className="w-3.5 h-3.5" /> Stage Passed
                </span>
              ) : (
                <span className="inline-flex items-center gap-1 px-3 py-1 rounded-full bg-destructive/15 border border-destructive/30 text-destructive text-xs font-bold uppercase tracking-wider">
                  <X className="w-3.5 h-3.5" /> Needs Practice
                </span>
              )}
            </div>
            <h2 className="text-2xl font-bold text-foreground">
              {isPassed ? "Foundations Solidified!" : "Reinforcement Required"}
            </h2>
            <p className="mt-2 text-sm text-foreground-muted leading-relaxed max-w-sm">
              {isPassed
                ? "You've successfully validated your understanding of these stage objectives and are ready to advance."
                : "A few key concepts require more support before advancing. Your AI Mentor has evaluated your session."}
            </p>
          </div>
        </div>

        {/* Detailed Metrics */}
        <div className="grid grid-cols-2 gap-4 border-t border-border/60 pt-6">
          <div className="p-4 rounded-2xl bg-muted/30 border border-border/40 text-center sm:text-left">
            <span className="text-xs text-foreground-muted block mb-1">Correct Answers</span>
            <span className="text-2xl font-bold">{correct} <span className="text-sm font-normal text-foreground-muted">/ {total}</span></span>
          </div>
          <div className="p-4 rounded-2xl bg-muted/30 border border-border/40 text-center sm:text-left">
            <span className="text-xs text-foreground-muted block mb-1">Passing Grade</span>
            <span className="text-2xl font-bold">70%</span>
          </div>
        </div>
      </section>

      {/* Right Panel: Actions & Navigation */}
      <section className="space-y-6 flex flex-col justify-between">
        
        {/* Scenario 1: Passed & Next Stage Available */}
        {isPassed && nextStage && (
          <div className="rounded-3xl border border-border bg-card p-6 shadow-md relative overflow-hidden flex-1 flex flex-col justify-between">
            <div className="absolute top-0 right-0 w-24 h-24 bg-success/5 rounded-full blur-2xl" />
            <div>
              <span className="text-[10px] text-success font-bold uppercase tracking-wider block mb-1">Up Next</span>
              <h3 className="text-xl font-bold text-foreground mb-1">
                Unlock: {nextStage.stageName}
              </h3>
              <p className="text-sm text-foreground-muted leading-relaxed">
                Step into Stage {nextStage.stageIndex + 1} of your personalized track. Excellent work.
              </p>
            </div>
            
            <Link href={`/student/contentPage/${nextStage.stageProgressId}`} className="mt-6">
              <Button className="w-full bg-success hover:bg-success/90 text-white font-bold h-12 rounded-xl flex items-center justify-center gap-2 transition-all shadow-md cursor-pointer">
                Unlock Next Stage <ArrowRight className="w-4 h-4" />
              </Button>
            </Link>
          </div>
        )}

        {/* Scenario 2: Passed & Completed Track (No Next Stage) */}
        {isPassed && !nextStage && (
          <div className="rounded-3xl border border-border bg-card p-6 shadow-md relative overflow-hidden flex-1 flex flex-col justify-between">
            <div className="absolute top-0 right-0 w-24 h-24 bg-primary/5 rounded-full blur-2xl" />
            <div>
              <div className="w-12 h-12 rounded-2xl bg-primary/10 flex items-center justify-center text-primary mb-4">
                <Award className="w-6 h-6" />
              </div>
              <h3 className="text-xl font-bold text-foreground mb-1">
                Track Finished!
              </h3>
              <p className="text-sm text-foreground-muted leading-relaxed">
                Outstanding! You have completed all stages of your career track. You are ready to tackle new challenges or specializations.
              </p>
            </div>
            
            <Link href="/student/homepage" className="mt-6">
              <Button className="w-full bg-primary hover:bg-primary/90 text-white font-bold h-12 rounded-xl flex items-center justify-center gap-2 transition-all shadow-md cursor-pointer">
                Back to Dashboard <ArrowRight className="w-4 h-4" />
              </Button>
            </Link>
          </div>
        )}

        {/* Scenario 3: Failed & Roadmap Adapted */}
        {!isPassed && roadmapAdapted && (
          <div className="rounded-3xl border border-amber-500/25 bg-amber-500/5 p-6 shadow-md relative overflow-hidden flex-1 flex flex-col justify-between">
            <div className="absolute top-0 right-0 w-24 h-24 bg-amber-500/5 rounded-full blur-2xl" />
            <div>
              <div className="w-10 h-10 rounded-xl bg-amber-500/10 flex items-center justify-center text-amber-500 mb-3">
                <Brain className="w-5 h-5" />
              </div>
              <span className="text-[10px] text-amber-500 font-bold uppercase tracking-wider block mb-1">AI Adaptation Enabled</span>
              <h3 className="text-lg font-bold text-foreground mb-1">
                Curriculum Resources Updated
              </h3>
              <p className="text-xs text-foreground-muted leading-relaxed">
                Your AI Mentor analyzed the topics you struggled with and has patched this stage's learning material with focused remediation modules.
              </p>
            </div>
            
            <div className="flex flex-col gap-2 mt-6">
              <Link href={`/student/contentPage/${stageProgressId}`}>
                <Button className="w-full bg-amber-500 hover:bg-amber-600 text-white font-bold h-11 rounded-xl flex items-center justify-center gap-2 transition-all cursor-pointer">
                  View Remediation Resources <ExternalLink className="w-4 h-4" />
                </Button>
              </Link>
              <Button 
                variant="outline" 
                onClick={onReset} 
                className="w-full border-border hover:bg-muted/30 text-foreground-secondary h-11 rounded-xl flex items-center justify-center gap-2 transition-all cursor-pointer"
              >
                <RefreshCw className="w-3.5 h-3.5" /> Try Assessment Again
              </Button>
            </div>
          </div>
        )}

        {/* Scenario 4: Failed & NOT Adapted */}
        {!isPassed && !roadmapAdapted && (
          <div className="rounded-3xl border border-border bg-card p-6 shadow-md relative overflow-hidden flex-1 flex flex-col justify-between">
            <div>
              <h3 className="text-xl font-bold text-foreground mb-1">
                Keep Pushing!
              </h3>
              <p className="text-sm text-foreground-muted leading-relaxed">
                You didn't reach the passing score this time. Review your notes, revisit the stage materials, and try the assessment again.
              </p>
            </div>
            
            <Button 
              onClick={onReset}
              className="w-full mt-6 bg-primary hover:bg-primary/95 text-white font-bold h-12 rounded-xl flex items-center justify-center gap-2 transition-all shadow-md cursor-pointer"
            >
              <RefreshCw className="w-4 h-4 animate-spin-reverse" /> Retake Assessment
            </Button>
          </div>
        )}
      </section>

      {/* AI Adaptation Alert Modal Dialog */}
      <Dialog open={showAdaptation} onOpenChange={setShowAdaptation}>
        <DialogContent className="sm:max-w-md border-border bg-card/95 backdrop-blur-md text-foreground">
          <DialogHeader className="flex flex-col items-center text-center">
            <div className="w-14 h-14 rounded-2xl bg-amber-500/10 text-amber-500 flex items-center justify-center mb-2 animate-bounce">
              <Brain className="w-8 h-8" />
            </div>
            <DialogTitle className="text-xl font-extrabold flex items-center gap-1.5 justify-center">
              AI Adaptation Active
            </DialogTitle>
            <DialogDescription className="text-foreground-muted text-sm mt-2 leading-relaxed">
              Your AI Mentor has automatically modified your syllabus. New targeted learning resources, explanations, and guides have been injected into this stage to help solidify your understanding of these concepts.
            </DialogDescription>
          </DialogHeader>
          <div className="flex flex-col gap-2 mt-4">
            <Button 
              onClick={() => {
                setShowAdaptation(false);
                router.push(`/student/contentPage/${stageProgressId}`);
              }}
              className="bg-amber-500 hover:bg-amber-600 text-white font-bold py-2.5 rounded-xl cursor-pointer"
            >
              Open Remediation Course
            </Button>
            <Button 
              variant="ghost" 
              onClick={() => setShowAdaptation(false)}
              className="text-foreground-muted hover:text-foreground cursor-pointer"
            >
              Close
            </Button>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
}
