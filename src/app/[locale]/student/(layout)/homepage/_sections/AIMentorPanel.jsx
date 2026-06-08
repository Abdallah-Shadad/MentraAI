"use client";

import { Sparkles, MessageCircle, Brain } from "lucide-react";
import { useState, useEffect } from "react";

const mentorMessages = {
  welcome: {
    title: "Welcome back!",
    message:
      "You are currently building your foundation. Let's continue where you left off.",
  },
  progress: {
    title: "Great progress!",
    message: "You're making excellent strides. Your consistency is paying off!",
  },
  struggling: {
    title: "Let's work through this",
    message:
      "I notice you might need some extra support with this section. That's completely normal.",
  },
  nearCompletion: {
    title: "Almost there!",
    message:
      "You're in the final stretch. Just a few more lessons to complete this track!",
  },
};

const AIMentorPanel = ({
  userName,
  currentModule,
  state = "welcome",
  recommendation,
}) => {
  const [isTyping, setIsTyping] = useState(true);

  const { title, message } = mentorMessages[state] || mentorMessages["welcome"];

  useEffect(() => {
    const timer = setTimeout(() => setIsTyping(false), 1500);
    return () => clearTimeout(timer);
  }, []);

  return (
    <div className="sticky top-4 z-20">
      <div className="relative overflow-hidden rounded-xl glass-card border border-primary/20 p-5 shadow-neon">
        {/* Glow decoration */}
        <div className="absolute -top-10 -right-10 w-32 h-32 bg-surface/20 rounded-full blur-3xl" />
        <div className="absolute -bottom-10 -left-10 w-24 h-24 bg-secondary/20 rounded-full blur-2xl" />

        <div className="relative z-10">
          {/* Header */}
          <div className="flex items-center gap-3 mb-4">
            <div className="relative">
              <div className="w-12 h-12 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center animate-float">
                <Brain className="w-6 h-6 text-foreground-text-foreground" />
              </div>

              <div className="absolute -bottom-1 -right-1 w-4 h-4 bg-success rounded-full border-2 border-card flex items-center justify-center">
                <Sparkles className="w-2 h-2 text-success-text-foreground" />
              </div>
            </div>

            <div>
              <h3 className="font-heading font-semibold text-foreground text-sm">
                Mentra AI Mentor
              </h3>

              <span className="text-xs text-foreground flex items-center gap-1">
                <span className="w-1.5 h-1.5 bg-success rounded-full animate-pulse" />
                Active now
              </span>
            </div>
          </div>

          {/* Message */}
          <div className="bg-muted/30 rounded-lg p-4 mb-4 border border-border/50">
            <p className="text-sm font-medium text-foreground mb-1">
              {title}, {userName}! 👋
            </p>

            <p className="text-sm text-foreground-muted leading-relaxed">
              {isTyping ? (
                <span className="flex items-center gap-1">
                  <span className="w-2 h-2 bg-surface rounded-full animate-pulse" />
                  <span className="w-2 h-2 bg-surface rounded-full animate-pulse delay-100" />
                  <span className="w-2 h-2 bg-surface rounded-full animate-pulse delay-200" />
                </span>
              ) : (
                message
              )}
            </p>
          </div>

          {/* Recommendation */}
          {recommendation && (
            <div className="flex items-start gap-2 p-3 rounded-lg bg-secondary/10 border border-secondary/20">
              <MessageCircle className="w-4 h-4 text-secondary mt-0.5 shrink-0" />

              <div>
                <span className="text-xs font-medium text-secondary block mb-1">
                  My Recommendation
                </span>

                <p className="text-xs text-foreground-muted">
                  {recommendation}
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

              <span className="text-xs font-medium text-foreground bg-muted/50 px-2 py-1 rounded">
                {currentModule}
              </span>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};

export default AIMentorPanel;
