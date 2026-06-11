"use client";

import React from "react";
import { useChatContext } from "./ChatProvider";
import ChatHeader from "./ChatHeader";
import ChatMessageList from "./ChatMessageList";
import ChatInput from "./ChatInput";
import { AlertCircle } from "lucide-react";

export default function ChatDrawer() {
  const { isOpen, isMinimized, isAIOnline, closeDrawer, openDrawer } = useChatContext();

  if (!isOpen) {
    return null;
  }

  return (
    <>
      {/* Backdrop overlay (dismisses chat on click) - only visible when drawer is fully open and not minimized */}
      {isOpen && !isMinimized && (
        <div
          className="fixed inset-0 bg-black/30 backdrop-blur-xs z-[9998] transition-opacity duration-300"
          onClick={closeDrawer}
        />
      )}

      {/* Main Drawer Panel */}
      <div
        className={`
          fixed top-0 right-0 h-screen w-full sm:w-[420px] z-[9999]
          bg-card border-l border-border flex flex-col
          shadow-[-8px_0_40px_rgba(0,0,0,0.15)]
          transition-transform duration-300 cubic-bezier(0.32, 0.72, 0, 1)
          ${isMinimized ? "translate-x-full" : "translate-x-0"}
        `}
      >

        {/* Chat window content */}
        <div className={`flex flex-col h-full w-full ${isMinimized ? "pointer-events-none opacity-40" : ""}`}>
          <ChatHeader />
          
          {/* AI Offline Warning Banner */}
          {isAIOnline === false && (
            <div className="flex items-center gap-2 p-3 bg-destructive/10 border-b border-destructive/20 text-destructive text-xs">
              <AlertCircle className="w-4 h-4 shrink-0" />
              <span>Atlas is temporarily offline. You can read history but sending messages is locked.</span>
            </div>
          )}

          {/* Messages Log container */}
          <div className="flex-1 min-h-0 relative overflow-hidden bg-background">
            <ChatMessageList />
          </div>

          {/* Message input bar */}
          <ChatInput />
        </div>
      </div>
    </>
  );
}
