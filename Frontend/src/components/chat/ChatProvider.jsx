"use client";

import React, { createContext, useContext, useMemo } from "react";
import { usePathname } from "next/navigation";
import { useChat } from "@/hooks/useChat";
import ChatBubble from "./ChatBubble";
import ChatDrawer from "./ChatDrawer";

const ChatContext = createContext(null);

const EXCLUDED_PATTERNS = [
  "/login",
  "/signup",
  "/onboarding"
];

export function ChatProvider({ children }) {
  const pathname = usePathname();

  // Parse path and strip language prefix /en, /ar, /fr, etc.
  const cleanPath = useMemo(() => {
    if (!pathname) return "";
    return pathname.replace(/^\/[a-zA-Z]{2}(-[a-zA-Z]{2,4})?(\/|$)/, "/");
  }, [pathname]);

  // Route guarding checks
  const isExcluded = useMemo(() => {
    return EXCLUDED_PATTERNS.some((pattern) => cleanPath.startsWith(pattern));
  }, [cleanPath]);

  // Auto-extraction of context properties from routing hierarchy
  const contextProps = useMemo(() => {
    const props = {};
    if (!cleanPath) return props;

    // Check if on contentPage
    const contentMatch = cleanPath.match(/^\/student\/contentPage\/([^/]+)/);
    if (contentMatch) {
      props.lessonId = contentMatch[1];
    }

    // Check if on roadmap page
    if (cleanPath.includes("/student/roadmap")) {
      props.stage = "roadmap";
    }

    return props;
  }, [cleanPath]);

  // Invoke master chat hook
  const chatController = useChat(contextProps);

  return (
    <ChatContext.Provider value={chatController}>
      {children}
      {!isExcluded && <ChatBubble />}
      {!isExcluded && <ChatDrawer />}
    </ChatContext.Provider>
  );
}

export function useChatContext() {
  const context = useContext(ChatContext);
  if (!context) {
    throw new Error("useChatContext must be used within a ChatProvider");
  }
  return context;
}
