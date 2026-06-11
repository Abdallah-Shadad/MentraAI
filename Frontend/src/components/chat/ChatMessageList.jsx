"use client";

import React, { useEffect, useRef } from "react";
import { useChatContext } from "./ChatProvider";
import ChatMessageBubble from "./ChatMessageBubble";
import ChatTypingIndicator from "./ChatTypingIndicator";

export default function ChatMessageList() {
  const { messages, isStreaming, isCreatingConversation, conversationId } = useChatContext();
  const containerRef = useRef(null);
  const messagesEndRef = useRef(null);
  
  // Track scroll lock status using ref to prevent stale closures
  const isLockedToBottomRef = useRef(true);

  // Monitor user scrolling to toggle lock
  const handleScroll = () => {
    const container = containerRef.current;
    if (!container) return;

    // A threshold of 100px buffer to detect if user is near bottom
    const threshold = 100;
    const isAtBottom =
      container.scrollHeight - container.scrollTop <= container.clientHeight + threshold;

    isLockedToBottomRef.current = isAtBottom;
  };

  const scrollToBottom = (instant = false) => {
    const container = containerRef.current;
    if (!container) return;

    if (instant) {
      container.scrollTop = container.scrollHeight;
    } else {
      messagesEndRef.current?.scrollIntoView({
        behavior: "smooth",
        block: "end",
      });
    }
  };

  // Bind scroll event listener
  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    container.addEventListener("scroll", handleScroll, { passive: true });
    return () => container.removeEventListener("scroll", handleScroll);
  }, []);

  // Force scroll-to-bottom on namespace transition or conversation initialization
  useEffect(() => {
    isLockedToBottomRef.current = true;
    scrollToBottom(true);
  }, [conversationId]);

  // Keep track of message count changes
  const prevMessagesLength = useRef(messages.length);

  useEffect(() => {
    const container = containerRef.current;
    if (!container) return;

    const hasNewMessage = messages.length > prevMessagesLength.current;
    prevMessagesLength.current = messages.length;

    // Detect if last message belongs to the user
    const lastMessageIsUser =
      messages.length > 0 && messages[messages.length - 1].role === "user";

    // Scroll rules:
    // 1. If user pushes a new message, force lock and smooth scroll.
    // 2. If streaming tokens and scroll lock is active, perform high-speed instant scroll to prevent rendering jitter.
    if (hasNewMessage && lastMessageIsUser) {
      isLockedToBottomRef.current = true;
      scrollToBottom(false);
    } else if (isStreaming && isLockedToBottomRef.current) {
      scrollToBottom(true);
    }
  }, [messages, isStreaming]);

  // Determine if we should render typing indicator
  const showTypingIndicator =
    isCreatingConversation ||
    (isStreaming &&
      messages.length > 0 &&
      messages[messages.length - 1].role === "atlas" &&
      messages[messages.length - 1].content.trim() === "");

  return (
    <div
      ref={containerRef}
      className="absolute inset-0 overflow-y-auto p-4 space-y-4 flex flex-col scrollbar-thin scrollbar-thumb-border scrollbar-track-transparent"
    >
      {messages.map((msg) => {
        // Hide the empty Atlas streaming message from list when showing typing indicator
        if (msg.role === "atlas" && msg.content === "" && isStreaming) {
          return null;
        }

        return <ChatMessageBubble key={msg.id} message={msg} />;
      })}

      {showTypingIndicator && <ChatTypingIndicator />}

      <div ref={messagesEndRef} className="h-2 shrink-0" />
    </div>
  );
}
