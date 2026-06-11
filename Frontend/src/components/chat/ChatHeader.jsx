"use client";

import React from "react";
import { Brain, X, ChevronRight, SquarePen } from "lucide-react";
import { useChatContext } from "./ChatProvider";

export default function ChatHeader() {
  const { 
    closeDrawer, 
    minimizeDrawer, 
    startNewConversation, 
    isAIOnline,
    conversationId
  } = useChatContext();

  return (
    <div className="h-16 px-4 border-b border-border bg-card flex items-center justify-between gap-3 shrink-0">
      {/* Mentor Identity */}
      <div className="flex items-center gap-2.5">
        <div className="relative">
          <div className="w-10 h-10 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center shadow-md">
            <Brain className="w-5 h-5 text-white" />
          </div>
          
          {/* Status Indicator */}
          <span className={`
            absolute -bottom-0.5 -right-0.5 w-3 h-3 rounded-full border-2 border-card
            ${isAIOnline === false ? "bg-destructive" : "bg-success animate-pulse"}
          `} />
        </div>

        <div className="flex flex-col">
          <span className="font-heading font-semibold text-foreground text-sm leading-tight">
            Atlas
          </span>
          <span className="text-[10px] text-foreground-muted leading-none mt-0.5 flex items-center gap-1">
            AI Learning Mentor
          </span>
        </div>
      </div>

      {/* Control Actions */}
      <div className="flex items-center gap-1">
        {/* New Chat Action */}
        {conversationId && (
          <button
            onClick={() => {
              if (window.confirm("Are you sure you want to start a new learning session? This will wipe conversation history.")) {
                startNewConversation();
              }
            }}
            className="p-1.5 rounded-lg text-foreground-muted hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
            title="New Chat"
          >
            <SquarePen className="w-4 h-4" />
          </button>
        )}

        {/* Minimize Action */}
        <button
          onClick={minimizeDrawer}
          className="p-1.5 rounded-lg text-foreground-muted hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
          title="Minimize Chat"
        >
          <ChevronRight className="w-4 h-4" />
        </button>

        {/* Close Action */}
        <button
          onClick={closeDrawer}
          className="p-1.5 rounded-lg text-foreground-muted hover:text-foreground hover:bg-muted transition-colors cursor-pointer"
          title="Close Chat"
        >
          <X className="w-4 h-4" />
        </button>
      </div>
    </div>
  );
}
