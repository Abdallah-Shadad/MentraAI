"use client";

import { Sparkles, MessageCircle, Brain, BookOpen } from "lucide-react";
import { useState, useEffect } from "react";

const AIMentorPanel = ({
  userProfile,
  trackData,
  roadmapData,
}) => {
  const [isTyping, setIsTyping] = useState(true);

  useEffect(() => {
    // Reset typing animation on mount or when data changes
    setIsTyping(true);
    const timer = setTimeout(() => setIsTyping(false), 1200);
    return () => clearTimeout(timer);
  }, [trackData, roadmapData]);

  // Extract user first name
  const firstName = userProfile?.data?.firstName || userProfile?.firstName || "Learner";

  // Build dynamic context helper
  const buildMentorContext = () => {
    const track = trackData?.data || trackData;
    const roadmap = roadmapData?.data || roadmapData;
    const stages = roadmap?.stages || [];
    const skillGaps = roadmap?.skillGaps || [];

    // Scenario A: No track selected
    if (!track || !track.careerTrackId) {
      return {
        title: "Welcome to MentraAI",
        message: "Let's get started on your tech journey! Choose a career path below to begin building your custom AI-driven curriculum.",
        recommendation: "Pick one of our curated tracks (like Full-Stack or Machine Learning) to unlock personalized lessons.",
        currentModule: "No Track Selected",
        hasGaps: false,
        gaps: []
      };
    }

    // Scenario B: Track selected but no roadmap generated yet
    const hasRoadmap = stages && stages.length > 0;
    if (!hasRoadmap) {
      return {
        title: "Ready to begin",
        message: `You selected the "${track.careerTrackName || "Career Track"}" track. Next, let's generate your custom AI roadmap to analyze skill gaps.`,
        recommendation: "Go to the Roadmap tab and click 'Generate Your Roadmap' to start learning.",
        currentModule: track.careerTrackName || "Active Track",
        hasGaps: false,
        gaps: []
      };
    }

    // Scenario C: Active in-progress
    const activeStage = stages.find((s) => s.status === "ACTIVE") || stages[0];
    const completedStages = stages.filter((s) => s.status === "COMPLETED") || [];
    const isCompleted = completedStages.length === stages.length;

    if (isCompleted) {
      return {
        title: "Congratulations!",
        message: `You have completed all stages of the "${track.careerTrackName}" track! You are ready to showcase your skills or choose a new track.`,
        recommendation: "Take a look at other learning paths to expand your skill set.",
        currentModule: "Track Completed 🎉",
        hasGaps: false,
        gaps: []
      };
    }

    // Actively learning
    const nextRec = skillGaps.length > 0
      ? `We detected a few skill gaps in your profile: ${skillGaps.slice(0, 3).join(", ")}. We have adjusted your stage resources to address these.`
      : "You're making excellent strides. Follow the learning path resources and pass the quiz to unlock the next stage!";

    return {
      title: "Great progress",
      message: `You are currently focusing on Stage ${activeStage.stageIndex + 1}: "${activeStage.stageName}". Your consistency is paying off!`,
      recommendation: nextRec,
      currentModule: activeStage.stageName,
      hasGaps: skillGaps.length > 0,
      gaps: skillGaps.slice(0, 3)
    };
  };

  const context = buildMentorContext();

  return (
    <div className="sticky top-4 z-20">
      <div className="relative overflow-hidden rounded-xl glass-card border border-primary/20 p-5 shadow-neon bg-card/90">
        {/* Glow decoration */}
        <div className="absolute -top-10 -right-10 w-32 h-32 bg-primary/10 rounded-full blur-3xl" />
        <div className="absolute -bottom-10 -left-10 w-24 h-24 bg-secondary/10 rounded-full blur-2xl" />

        <div className="relative z-10">
          {/* Header */}
          <div className="flex items-center gap-3 mb-4">
            <div className="relative">
              <div className="w-12 h-12 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center animate-float">
                <Brain className="w-6 h-6 text-white" />
              </div>

              <div className="absolute -bottom-1 -right-1 w-4 h-4 bg-success rounded-full border-2 border-card flex items-center justify-center">
                <Sparkles className="w-2 h-2 text-white" />
              </div>
            </div>

            <div>
              <h3 className="font-heading font-semibold text-foreground text-sm">
                Mentra AI Mentor
              </h3>

              <span className="text-xs text-foreground-light flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-success rounded-full animate-pulse" />
                Active now
              </span>
            </div>
          </div>

          {/* Message */}
          <div className="bg-muted/30 rounded-lg p-4 mb-4 border border-border/50">
            <p className="text-sm font-semibold text-foreground mb-1">
              {context.title}, {firstName}! 👋
            </p>

            <div className="text-sm text-foreground-muted leading-relaxed min-h-[40px]">
              {isTyping ? (
                <span className="flex items-center gap-1 py-2">
                  <span className="w-2 h-2 bg-primary rounded-full animate-bounce" />
                  <span className="w-2 h-2 bg-primary rounded-full animate-bounce delay-100" />
                  <span className="w-2 h-2 bg-primary rounded-full animate-bounce delay-200" />
                </span>
              ) : (
                context.message
              )}
            </div>
          </div>

          {/* Gaps / Skills tag cloud */}
          {!isTyping && context.hasGaps && (
            <div className="mb-4">
              <div className="text-[10px] uppercase tracking-wider text-foreground-muted mb-2 font-semibold">
                Identified Skill Gaps
              </div>
              <div className="flex flex-wrap gap-1.5">
                {context.gaps.map((gap, i) => (
                  <span
                    key={i}
                    className="px-2 py-0.5 text-xs rounded-md bg-destructive/10 text-destructive border border-destructive/20"
                  >
                    {gap}
                  </span>
                ))}
              </div>
            </div>
          )}

          {/* Recommendation */}
          {!isTyping && context.recommendation && (
            <div className="flex items-start gap-2.5 p-3 rounded-lg bg-secondary/10 border border-secondary/20">
              <MessageCircle className="w-4 h-4 text-secondary mt-0.5 shrink-0" />

              <div>
                <span className="text-xs font-semibold text-secondary block mb-1">
                  Mentor Recommendation
                </span>

                <p className="text-xs text-foreground-muted leading-relaxed">
                  {context.recommendation}
                </p>
              </div>
            </div>
          )}

          {/* Current Focus */}
          <div className="mt-4 pt-4 border-t border-border/50">
            <div className="flex items-center justify-between">
              <span className="text-xs text-foreground-muted">
                Currently focusing on
              </span>

              <span className="text-xs font-semibold text-foreground bg-muted/60 px-2 py-1 rounded-md border border-border/40">
                {context.currentModule}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AIMentorPanel;
