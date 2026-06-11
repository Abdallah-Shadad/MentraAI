"use client";

import React from "react";

export default function ChatTypingIndicator() {
  return (
    <div className="flex w-full justify-start animate-slide-up">
      <div className="max-w-[80%] px-4 py-3 rounded-2xl bg-surface-elevated text-foreground border border-border rounded-bl-sm shadow-sm flex flex-col gap-1.5">
        <div className="flex items-center gap-1.5 py-1">
          {/* Pulsing Dots with Staggered Delays */}
          <span className="w-2 h-2 rounded-full bg-primary animate-pulse" />
          <span className="w-2 h-2 rounded-full bg-primary animate-pulse [animation-delay:0.2s]" />
          <span className="w-2 h-2 rounded-full bg-primary animate-pulse [animation-delay:0.4s]" />
        </div>
        <span className="text-[10px] text-foreground-muted font-medium">
          Atlas is thinking...
        </span>
      </div>
    </div>
  );
}
