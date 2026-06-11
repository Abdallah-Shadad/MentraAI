"use client";

import React from "react";
import { Sparkles } from "lucide-react";
import { useChatContext } from "./ChatProvider";

export default function ChatBubble() {
  const { isOpen, isMinimized, openDrawer, isAIOnline, isStreaming } = useChatContext();

  // Hide bubble only when the drawer is fully open and NOT minimized
  if (isOpen && !isMinimized) {
    return null;
  }

  return (
    <button
      onClick={openDrawer}
      className={`
        fixed bottom-6 right-6 z-[9998]
        w-14 h-14 rounded-full
        flex items-center justify-center
        text-white cursor-pointer
        transition-all duration-300 ease-out
        hover:scale-110 active:scale-95
        bg-linear-to-br from-primary to-secondary
        shadow-[0_0_20px_rgba(139,92,246,0.4)]
        hover:shadow-[0_0_25px_rgba(139,92,246,0.6)]
        animate-bounce-in
      `}
      aria-label="Open Atlas AI Mentor"
    >
      <div className="relative flex items-center justify-center w-full h-full">
        <Sparkles className="w-6 h-6 animate-float" />
        
        {/* Dynamic health/streaming notification badge */}
        {isStreaming ? (
          <span className="absolute -top-1 -right-1 flex h-4 w-4">
            <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-violet-400 opacity-75"></span>
            <span className="relative inline-flex rounded-full h-4 w-4 bg-violet-500 border-2 border-white dark:border-card flex items-center justify-center">
              <span className="w-1.5 h-1.5 rounded-full bg-white animate-pulse" />
            </span>
          </span>
        ) : (
          isAIOnline !== false && (
            <span className="absolute top-0 right-0 flex h-3.5 w-3.5">
              <span className="animate-ping absolute inline-flex h-full w-full rounded-full bg-success opacity-75"></span>
              <span className="relative inline-flex rounded-full h-3.5 w-3.5 bg-success border-2 border-white dark:border-card"></span>
            </span>
          )
        )}
      </div>
    </button>
  );
}
