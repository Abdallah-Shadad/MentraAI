"use client";

import { useState, useEffect, useRef } from "react";
import TrackRecommendationCard from "../_components/TrackRecommendationCard";
import ErrorState from "@/components/reusable/ErrorState";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";

import { useTracksRecommended } from "@/hooks/useCareerTrack";
import {
  Brain,
  CheckCircle2,
  Sparkles,
  Loader2,
  Bot,
  Cpu,
  Database,
  Compass,
  Terminal,
  Activity,
} from "lucide-react";

const STAGES = [
  {
    id: 1,
    title: "Gathering Profile Insights",
    description: "Accessing student profiles, preferences, and competencies.",
    icon: Database,
  },
  {
    id: 2,
    title: "Evaluating Quiz Performance",
    description: "Analyzing knowledge gaps, quiz scores, and subject mastery.",
    icon: Brain,
  },
  {
    id: 3,
    title: "Invoking Cognitive Models",
    description: "Querying AI-Gateway matching engines for career tracks.",
    icon: Cpu,
  },
  {
    id: 4,
    title: "Aligning Industry Tracks",
    description: "Evaluating career paths against student strengths.",
    icon: Compass,
  },
  {
    id: 5,
    title: "Finalizing Pathways",
    description: "Computing fit scores, skill overlaps, and roadmaps.",
    icon: Sparkles,
  },
];

const SIMULATED_LOGS = [
  { text: "System initialization: OK", type: "system", delay: 100 },
  { text: "Establishing connection to MentraAI backend gateway...", type: "system", delay: 400 },
  { text: "API channel validated (SSL: secure, RTT: 28ms).", type: "system", delay: 800 },
  { text: "Fetching student academic profile records...", type: "profile", delay: 1300 },
  { text: "Profile record found (ID: student_842a). Preferred domain: Software Engineering.", type: "profile", delay: 1800 },
  { text: "Analyzing historic quiz attempts...", type: "quiz", delay: 2400 },
  { text: "Attempt ID: qz_772 - Score: 62% (Found strong CSS & React basics).", type: "quiz", delay: 3000 },
  { text: "Attempt ID: qz_991 - Score: 45% (Identified gaps in state management).", type: "quiz", delay: 3600 },
  { text: "Cognitive gap analysis completed.", type: "quiz", delay: 4200 },
  { text: "Sending parameters to recommendation engine models...", type: "ai", delay: 4800 },
  { text: "Running AI Vector similarity search against career specializations...", type: "ai", delay: 5400 },
  { text: "Calculating fit score matrices for 'Frontend Developer'...", type: "matcher", delay: 6000 },
  { text: "Calculating fit score matrices for 'Fullstack Developer'...", type: "matcher", delay: 6600 },
  { text: "Calculating fit score matrices for 'UI/UX Engineer'...", type: "matcher", delay: 7200 },
  { text: "Calculating fit score matrices for 'Backend Engineer'...", type: "matcher", delay: 7800 },
  { text: "Matching skills overlap for candidate tracks...", type: "matcher", delay: 8400 },
  { text: "Determining skills to acquire: [Next.js, Tailwind, System Design]...", type: "matcher", delay: 9000 },
  { text: "Synthesizing custom learning paths...", type: "builder", delay: 9600 },
  { text: "Compiling JSON recommendation schema payload...", type: "builder", delay: 10200 },
  { text: "Validating recommendations integrity...", type: "builder", delay: 10800 },
  { text: "AI recommended tracks compiled successfully.", type: "system", delay: 11400 },
  { text: "Handing payload to UI presentation layer.", type: "system", delay: 12000 },
];

function TrackRecommendationSkeleton() {
  return (
    <div className="relative overflow-hidden rounded-lg border border-border bg-card p-6 shadow-shadow-card animate-pulse min-h-[380px] flex flex-col justify-between">
      {/* Glow effect matching original card */}
      <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(124,58,237,0.02),transparent_40%)]" />
      
      <div className="relative z-10 flex flex-col h-full justify-between">
        <div>
          {/* Header Skeleton */}
          <div className="flex flex-col md:flex-row items-center gap-2 justify-between">
            <div className="flex items-center gap-3 w-full">
              {/* Icon placeholder */}
              <div className="flex size-12 shrink-0 items-center justify-center rounded-2xl bg-muted" />
              {/* Title / Subtitle placeholder */}
              <div className="space-y-2 w-full max-w-[120px]">
                <div className="h-3 bg-muted rounded w-2/3" />
                <div className="h-5 bg-muted rounded w-full" />
              </div>
            </div>
            {/* Score Ring placeholder */}
            <div className="relative size-20 shrink-0">
              <svg className="size-20 -rotate-90" viewBox="0 0 100 100">
                <circle
                  cx="50"
                  cy="50"
                  r="40"
                  stroke="var(--border)"
                  strokeWidth="8"
                  fill="none"
                  className="opacity-20"
                />
              </svg>
              <div className="absolute inset-0 flex flex-col items-center justify-center">
                <span className="text-xl font-bold bg-muted text-transparent rounded w-8 h-6" />
                <span className="text-[10px] text-foreground-muted mt-1">MATCH</span>
              </div>
            </div>
          </div>

          {/* Reasoning box placeholder */}
          <div className="mt-5 rounded-xl border border-border bg-surface-elevated/50 p-4 space-y-2">
            <div className="h-3.5 bg-muted rounded w-full" />
            <div className="h-3.5 bg-muted rounded w-5/6" />
          </div>

          {/* Skills Grid placeholder */}
          <div className="mt-6 grid gap-4 grid-cols-2">
            <div>
              <div className="h-4 bg-muted rounded w-2/3 mb-3" />
              <div className="flex flex-wrap gap-2">
                <div className="h-7 bg-success/5 border border-success/15 rounded-full w-20" />
                <div className="h-7 bg-success/5 border border-success/15 rounded-full w-24" />
              </div>
            </div>
            <div>
              <div className="h-4 bg-muted rounded w-2/3 mb-3" />
              <div className="flex flex-wrap gap-2">
                <div className="h-7 bg-muted rounded-full w-16" />
                <div className="h-7 bg-muted rounded-full w-20" />
              </div>
            </div>
          </div>
        </div>

        {/* Button placeholder */}
        <div className="mt-6 h-10 bg-primary/10 border border-primary/20 rounded-md w-full" />
      </div>
    </div>
  );
}

export default function TracksRecommendationList({ isOpen, onOpenChange }) {
  const { data, isLoading, isError, error } = useTracksRecommended();

  const [visibleLogs, setVisibleLogs] = useState([]);
  const [currentStage, setCurrentStage] = useState(1);
  const [progress, setProgress] = useState(0);
  const terminalEndRef = useRef(null);

  useEffect(() => {
    if (!isLoading) return;

    let timers = [];
    
    // Reset states
    setVisibleLogs([]);
    setCurrentStage(1);
    setProgress(0);

    // Schedule log stream
    SIMULATED_LOGS.forEach((log) => {
      const timer = setTimeout(() => {
        setVisibleLogs((prev) => [...prev, log]);
        
        // Sync active stage with the current logs log group
        if (log.delay >= 0 && log.delay < 1500) setCurrentStage(1);
        else if (log.delay >= 1500 && log.delay < 3500) setCurrentStage(2);
        else if (log.delay >= 3500 && log.delay < 6000) setCurrentStage(3);
        else if (log.delay >= 6000 && log.delay < 8800) setCurrentStage(4);
        else if (log.delay >= 8800) setCurrentStage(5);
        
      }, log.delay);
      timers.push(timer);
    });

    // Simulate progress bar increase
    const progressInterval = setInterval(() => {
      setProgress((prev) => {
        if (prev >= 98) return 98;
        // Fast early on, slows down to mimic fetching delay completion
        const increment = prev < 50 ? 1.8 : prev < 80 ? 0.6 : 0.15;
        return Math.min(prev + increment, 98);
      });
    }, 100);

    return () => {
      timers.forEach(clearTimeout);
      clearInterval(progressInterval);
    };
  }, [isLoading]);

  useEffect(() => {
    if (terminalEndRef.current) {
      terminalEndRef.current.scrollIntoView({ behavior: "smooth" });
    }
  }, [visibleLogs]);

  const getLogColor = (type) => {
    switch (type) {
      case "system": return "text-cyan-400 dark:text-cyan-400";
      case "profile": return "text-emerald-400 dark:text-emerald-400";
      case "quiz": return "text-yellow-400 dark:text-yellow-400";
      case "ai": return "text-pink-400 dark:text-pink-400";
      case "matcher": return "text-violet-400 dark:text-violet-400";
      case "builder": return "text-blue-400 dark:text-blue-400";
      default: return "text-slate-300 dark:text-slate-300";
    }
  };

  const tracks = data?.data?.recommendedTracks || [];

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="min-w-[calc(100%-10rem)] h-[95vh] overflow-y-auto border-border [&>button]:text-destructive [&>button]:hover:text-destructive-foreground [&>button]:cursor-pointer">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold text-center text-foreground">
            Select a Track
          </DialogTitle>
          <DialogDescription className="text-foreground-muted text-center">
            Choose a track to start your learning journey
          </DialogDescription>
        </DialogHeader>

        {/* Custom Keyframes injection */}
        <style dangerouslySetInnerHTML={{ __html: `
          @keyframes scanline {
            0% { left: -100%; }
            100% { left: 100%; }
          }
          .animate-scan {
            animation: scanline 2.5s linear infinite;
          }
          .terminal-scroll::-webkit-scrollbar {
            width: 6px;
          }
          .terminal-scroll::-webkit-scrollbar-track {
            background: rgba(0,0,0,0.2);
            border-radius: 4px;
          }
          .terminal-scroll::-webkit-scrollbar-thumb {
            background: rgba(255,255,255,0.1);
            border-radius: 4px;
          }
          .terminal-scroll::-webkit-scrollbar-thumb:hover {
            background: rgba(255,255,255,0.2);
          }
        `}} />

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mt-6">
          {isError && (
            <div className="col-span-full">
              <ErrorState message={error?.response?.data?.error?.message} />
            </div>
          )}

          {isLoading && (
            <div className="col-span-full space-y-8">
              {/* AI matching system HUD console */}
              <div className="w-full bg-slate-900/5 dark:bg-slate-950/40 border border-border rounded-2xl p-6 relative overflow-hidden">
                {/* Glowing grid background */}
                <div className="absolute inset-0 bg-[linear-gradient(rgba(124,58,237,0.01)_1px,transparent_1px),linear-gradient(90deg,rgba(124,58,237,0.01)_1px,transparent_1px)] bg-[size:20px_20px] pointer-events-none" />
                <div className="absolute inset-0 bg-[radial-gradient(ellipse_60%_50%_at_50%_0%,rgba(124,58,237,0.05),transparent)] pointer-events-none" />

                <div className="relative z-10 grid grid-cols-1 lg:grid-cols-12 gap-8 items-center">
                  {/* Left Column: Progress status center */}
                  <div className="lg:col-span-5 flex flex-col items-center text-center lg:items-start lg:text-left space-y-5">
                    <div className="flex items-center gap-3">
                      <div className="relative flex size-12 items-center justify-center rounded-2xl bg-primary/10 border border-primary/20 text-primary">
                        <Activity className="size-6 animate-pulse" />
                        <span className="absolute -top-1 -right-1 flex h-3.5 w-3.5">
                          <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-primary opacity-75"></span>
                          <span className="relative inline-flex rounded-full h-3.5 w-3.5 bg-primary"></span>
                        </span>
                      </div>
                      <div>
                        <div className="text-xs font-bold uppercase tracking-widest text-primary flex items-center gap-1.5 justify-center lg:justify-start">
                          <Sparkles className="size-3" /> MentraAI Cognitive Engine
                        </div>
                        <h3 className="text-xl font-extrabold text-foreground">
                          Synthesizing Career Tracks
                        </h3>
                      </div>
                    </div>

                    {/* Progress Bar */}
                    <div className="w-full space-y-2">
                      <div className="flex justify-between text-xs font-semibold">
                        <span className="text-foreground-secondary">Analysis Progress</span>
                        <span className="text-primary font-mono">{Math.floor(progress)}%</span>
                      </div>
                      <div className="h-2 w-full bg-muted rounded-full overflow-hidden relative border border-border">
                        <div 
                          className="h-full bg-gradient-to-r from-primary via-secondary to-primary rounded-full transition-all duration-100 ease-out relative"
                          style={{ width: `${progress}%` }}
                        >
                          <div className="absolute top-0 bottom-0 bg-[linear-gradient(90deg,transparent_0%,rgba(255,255,255,0.4)_50%,transparent_100%)] animate-scan" style={{ width: '80px' }} />
                        </div>
                      </div>
                    </div>

                    {/* Active Stage Panel */}
                    <div className="p-4 rounded-xl bg-card border border-border w-full flex items-start gap-3.5 shadow-sm text-left">
                      {(() => {
                        const ActiveIcon = STAGES[currentStage - 1]?.icon || Loader2;
                        return (
                          <>
                            <div className="p-2.5 rounded-lg bg-primary/10 border border-primary/20 text-primary shrink-0">
                              <ActiveIcon className="size-5 animate-pulse" />
                            </div>
                            <div className="space-y-0.5">
                              <div className="text-[10px] font-bold uppercase text-foreground-muted tracking-wider">
                                Current Process (Step {currentStage}/5)
                              </div>
                              <div className="text-sm font-semibold text-foreground">
                                {STAGES[currentStage - 1]?.title}
                              </div>
                              <div className="text-xs text-foreground-secondary leading-relaxed">
                                {STAGES[currentStage - 1]?.description}
                              </div>
                            </div>
                          </>
                        );
                      })()}
                    </div>
                  </div>

                  {/* Divider */}
                  <div className="hidden lg:block lg:col-span-1 h-32 w-px bg-border justify-self-center" />

                  {/* Right Column: Simulated Terminal Stream */}
                  <div className="lg:col-span-6 w-full">
                    <div className="rounded-xl border border-slate-800 bg-slate-950 shadow-2xl overflow-hidden flex flex-col">
                      <div className="flex items-center justify-between px-4 py-2.5 bg-slate-900 border-b border-slate-800/80">
                        <div className="flex items-center gap-1.5">
                          <span className="size-2 rounded-full bg-rose-500/80 inline-block" />
                          <span className="size-2 rounded-full bg-amber-500/80 inline-block" />
                          <span className="size-2 rounded-full bg-emerald-500/80 inline-block" />
                        </div>
                        <span className="text-[10px] font-mono text-slate-400 font-semibold tracking-wider flex items-center gap-1.5 select-none">
                          <Terminal className="size-3" /> mentra-ai-engine.sh
                        </span>
                        <div className="w-8" />
                      </div>
                      
                      <div className="p-4 font-mono text-[11px] leading-relaxed text-slate-300 min-h-[140px] max-h-[140px] overflow-y-auto terminal-scroll">
                        <div className="space-y-1">
                          {visibleLogs.map((log, index) => (
                            <div key={index} className="flex items-start gap-1">
                              <span className="text-slate-500 select-none shrink-0">{`>`}</span>
                              <span className={`${getLogColor(log.type)} break-all`}>{log.text}</span>
                            </div>
                          ))}
                          <div className="flex items-center gap-1">
                            <span className="text-slate-500 select-none shrink-0">$</span>
                            <span className="text-primary font-bold animate-pulse">█</span>
                          </div>
                          <div ref={terminalEndRef} />
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </div>

              {/* Skeleton Grid for recommended cards */}
              <div className="space-y-4">
                <div className="flex items-center justify-between border-b border-border/80 pb-2">
                  <h4 className="text-xs font-bold uppercase tracking-wider text-foreground-muted flex items-center gap-2">
                    <Bot className="size-4 text-primary" /> Preparing Layout Grid
                  </h4>
                  <span className="text-[10px] text-foreground-muted italic">Awaiting backend response payload...</span>
                </div>
                <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
                  <TrackRecommendationSkeleton />
                  <TrackRecommendationSkeleton />
                  <TrackRecommendationSkeleton />
                  <TrackRecommendationSkeleton />
                </div>
              </div>
            </div>
          )}

          {!isLoading && tracks?.map((track) => (
            <TrackRecommendationCard
              key={track.careerTrackId}
              track={track}
            />
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}

