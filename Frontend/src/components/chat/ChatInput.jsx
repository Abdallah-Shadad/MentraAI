"use client";

import React, { useState, useRef, useEffect } from "react";
import { ArrowUp } from "lucide-react";
import { useChatContext } from "./ChatProvider";

export default function ChatInput() {
  const { sendMessage, isStreaming, isCreatingConversation, isAIOnline } = useChatContext();
  const [text, setText] = useState("");
  const textareaRef = useRef(null);

  const isBlocked = isStreaming || isCreatingConversation || isAIOnline === false;

  // Auto-resize textarea row count based on content height
  useEffect(() => {
    const textarea = textareaRef.current;
    if (!textarea) return;
    
    // Reset height
    textarea.style.height = "auto";
    // Set to scrollHeight up to max 120px
    textarea.style.height = `${Math.min(textarea.scrollHeight, 120)}px`;
  }, [text]);

  const handleSubmit = async (e) => {
    e?.preventDefault();
    if (isBlocked || text.trim() === "") return;

    const queryToSend = text;
    setText(""); // Optimistically clear input
    
    // Reset textarea height to default
    if (textareaRef.current) {
      textareaRef.current.style.height = "auto";
    }

    await sendMessage(queryToSend);
  };

  const handleKeyDown = (e) => {
    if (e.key === "Enter" && !e.shiftKey) {
      e.preventDefault();
      handleSubmit();
    }
  };

  return (
    <form
      onSubmit={handleSubmit}
      className="p-3 border-t border-border bg-card flex flex-col gap-1.5 shrink-0"
    >
      <div className="flex items-end gap-2 rounded-xl border border-border bg-surface-elevated/60 px-3 py-2 focus-within:border-primary/50 focus-within:ring-2 focus-within:ring-primary/10 transition-all">
        <textarea
          ref={textareaRef}
          value={text}
          onChange={(e) => setText(e.target.value.slice(0, 2000))}
          onKeyDown={handleKeyDown}
          placeholder={isAIOnline === false ? "Atlas is offline..." : "Ask Atlas anything..."}
          disabled={isBlocked}
          rows={1}
          className="bg-transparent text-sm text-foreground flex-1 resize-none outline-hidden placeholder:text-foreground-muted max-h-[120px] py-1.5 align-bottom"
        />

        <button
          type="submit"
          disabled={isBlocked || text.trim() === ""}
          className={`
            w-8 h-8 rounded-lg flex items-center justify-center shrink-0 mb-0.5
            transition-all duration-200 cursor-pointer
            ${
              isBlocked || text.trim() === ""
                ? "bg-muted text-foreground-muted cursor-not-allowed"
                : "bg-linear-to-br from-primary to-secondary text-white shadow-sm hover:scale-105 active:scale-95"
            }
          `}
          title="Send Message"
        >
          <ArrowUp className="w-4 h-4" />
        </button>
      </div>

      <div className="flex justify-between items-center px-1 text-[10px] text-foreground-muted">
        <span>Press Enter to send, Shift+Enter for new line</span>
        <span>{text.length} / 2000</span>
      </div>
    </form>
  );
}
