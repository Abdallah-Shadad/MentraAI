"use client";

import { useState, useEffect, useRef, useCallback } from "react";
import { 
  createConversation, 
  deleteConversation, 
  checkChatHealth, 
  streamMessage 
} from "../services/chat.service";
import { getUserProfile } from "../services/user.service";

const ATLAS_WELCOME_MSG = {
  id: "atlas-welcome",
  role: "atlas",
  content: "Hi! I'm **Atlas**, your educational assistant. Ask me anything about your studies, course concepts, or quiz results. I'm here to support you! 🚀",
  timestamp: Date.now(),
  isStreaming: false
};

export const useChat = (contextProps = {}) => {
  const [isOpen, setIsOpen] = useState(false);
  const [isMinimized, setIsMinimized] = useState(false);
  const [messages, setMessages] = useState([]);
  const [conversationId, setConversationId] = useState(null);
  
  // Async states
  const [isCreatingConversation, setIsCreatingConversation] = useState(false);
  const [isStreaming, setIsStreaming] = useState(false);
  const [isAIOnline, setIsAIOnline] = useState(null); // null = checking, true = online, false = offline
  const [streamError, setStreamError] = useState(null);

  // Connection Abort controller ref
  const abortControllerRef = useRef(null);

  // Dynamic storage keys based on active context
  const getStorageKeys = useCallback(() => {
    let namespace = "global";
    if (contextProps.lessonId) {
      namespace = `lesson_${contextProps.lessonId}`;
    } else if (contextProps.stage) {
      namespace = `stage_${contextProps.stage}`;
    }
    return {
      idKey: `mentra_chat_session_${namespace}_id`,
      msgsKey: `mentra_chat_session_${namespace}_msgs`
    };
  }, [contextProps.lessonId, contextProps.stage]);

  // Sync messages helper
  const updateMessages = useCallback((newMessages) => {
    setMessages(newMessages);
    if (typeof window !== "undefined") {
      const { msgsKey } = getStorageKeys();
      sessionStorage.setItem(msgsKey, JSON.stringify(newMessages));
    }
  }, [getStorageKeys]);

  // Abort any active streaming connection
  const abortStream = useCallback(() => {
    if (abortControllerRef.current) {
      abortControllerRef.current.abort();
      abortControllerRef.current = null;
      setIsStreaming(false);
    }
  }, []);

  // Handle Load & Namespace Switching
  useEffect(() => {
    if (typeof window === "undefined") return;
    
    const { idKey, msgsKey } = getStorageKeys();
    const savedId = sessionStorage.getItem(idKey);
    const savedMsgs = sessionStorage.getItem(msgsKey);
    
    // Auto-abort stream to prevent message bleed on navigation
    abortStream();
    
    if (savedId) {
      setConversationId(savedId);
    } else {
      setConversationId(null);
    }

    if (savedMsgs) {
      try {
        setMessages(JSON.parse(savedMsgs));
      } catch (e) {
        setMessages([ATLAS_WELCOME_MSG]);
      }
    } else {
      setMessages([ATLAS_WELCOME_MSG]);
    }
    setStreamError(null);

    // Verify AI status on mount and namespace transitions
    checkChatHealth().then((status) => {
      setIsAIOnline(status);
    });
  }, [getStorageKeys, abortStream]);

  // Handle Close & Minimize Events to free socket connections
  const closeDrawer = useCallback(() => {
    abortStream();
    setIsOpen(false);
    setIsMinimized(false);
  }, [abortStream]);

  const minimizeDrawer = useCallback(() => {
    // Keep stream active when minimized (user can browse while Atlas types)
    setIsMinimized(true);
  }, []);

  const openDrawer = useCallback(() => {
    setIsOpen(true);
    setIsMinimized(false);
    // Refresh health status when drawer opens
    checkChatHealth().then((status) => {
      setIsAIOnline(status);
    });
  }, []);

  // Clear session / Start New Conversation
  const startNewConversation = useCallback(async () => {
    abortStream();
    
    // If there is an existing conversationId, we delete it from DB (best-effort)
    if (conversationId) {
      try {
        await deleteConversation(conversationId);
      } catch (err) {
        console.warn("Failed to delete conversation:", err);
      }
    }

    setConversationId(null);
    updateMessages([ATLAS_WELCOME_MSG]);
    setStreamError(null);
    if (typeof window !== "undefined") {
      const { idKey, msgsKey } = getStorageKeys();
      sessionStorage.removeItem(idKey);
      sessionStorage.removeItem(msgsKey);
    }
  }, [conversationId, abortStream, updateMessages, getStorageKeys]);

  // Send Message workflow
  const sendMessage = useCallback(async (queryText) => {
    if (!queryText || queryText.trim() === "") return;
    if (isStreaming || isCreatingConversation) return;

    setStreamError(null);
    abortStream();

    // 1. Pre-flight silent token check to guarantee cookie validation before SSE fetch
    try {
      await getUserProfile();
    } catch (err) {
      console.error("Pre-flight authentication check failed:", err);
      setStreamError("Authentication expired. Please log in again.");
      if (typeof window !== "undefined") {
        window.location.href = "/en/register/Login";
      }
      return;
    }

    let currentConvId = conversationId;

    // Optimistically push the user message
    const userMsg = {
      id: Date.now().toString(),
      role: "user",
      content: queryText.trim(),
      timestamp: Date.now(),
      isStreaming: false
    };
    
    const updatedMessagesList = [...messages, userMsg];
    updateMessages(updatedMessagesList);

    // Initial message triggers conversation creation if conversationId is missing
    if (!currentConvId) {
      setIsCreatingConversation(true);
      try {
        const title = queryText.slice(0, 80);
        const res = await createConversation(title);
        
        if (res?.data?.conversationId) {
          currentConvId = res.data.conversationId;
          setConversationId(currentConvId);
          if (typeof window !== "undefined") {
            const { idKey } = getStorageKeys();
            sessionStorage.setItem(idKey, currentConvId);
          }
        } else {
          throw new Error("Unable to establish conversation session.");
        }
      } catch (err) {
        console.error(err);
        setStreamError(err.message || "Failed to initialize conversation.");
        setIsCreatingConversation(false);
        return;
      } finally {
        setIsCreatingConversation(false);
      }
    }

    // Set streaming states
    setIsStreaming(true);
    const streamingMsgId = "streaming-msg-" + Date.now();
    const tempStreamingMsg = {
      id: streamingMsgId,
      role: "atlas",
      content: "",
      timestamp: Date.now(),
      isStreaming: true
    };
    
    // Add empty response block for token appending
    const listWithStreaming = [...updatedMessagesList, tempStreamingMsg];
    updateMessages(listWithStreaming);

    // Create abort controller for this stream session
    const controller = new AbortController();
    abortControllerRef.current = controller;

    // Construct backend request combining context properties
    const requestPayload = {
      query: queryText.trim(),
      careerTrack: contextProps.careerTrack || null,
      stage: contextProps.stage || null,
      lessonId: contextProps.lessonId || null,
      quizDetails: contextProps.quizDetails || null,
      quizScore: contextProps.quizScore !== undefined ? contextProps.quizScore : null
    };

    // Execute streaming call
    await streamMessage(
      currentConvId,
      requestPayload,
      {
        onToken: (token) => {
          setMessages((prevMsgs) => {
            const nextList = prevMsgs.map((msg) => {
              if (msg.id === streamingMsgId) {
                return { ...msg, content: msg.content + token };
              }
              return msg;
            });
            // Update sessionStorage with current stream state
            if (typeof window !== "undefined") {
              const { msgsKey } = getStorageKeys();
              sessionStorage.setItem(msgsKey, JSON.stringify(nextList));
            }
            return nextList;
          });
        },
        onError: (err) => {
          setStreamError(err.message || "Error reading message stream.");
          setIsStreaming(false);
        },
        signal: controller.signal
      }
    );

    // Finalize stream message state
    setMessages((prevMsgs) => {
      const nextList = prevMsgs.map((msg) => {
        if (msg.id === streamingMsgId) {
          return { ...msg, isStreaming: false };
        }
        return msg;
      });
      if (typeof window !== "undefined") {
        const { msgsKey } = getStorageKeys();
        sessionStorage.setItem(msgsKey, JSON.stringify(nextList));
      }
      return nextList;
    });

    setIsStreaming(false);
    abortControllerRef.current = null;
  }, [conversationId, messages, isStreaming, isCreatingConversation, contextProps, abortStream, updateMessages, getStorageKeys]);

  // Effect cleanup for component unmounting to prevent memory leaks
  useEffect(() => {
    return () => {
      abortStream();
    };
  }, [abortStream]);

  return {
    isOpen,
    isMinimized,
    messages,
    conversationId,
    isCreatingConversation,
    isStreaming,
    isAIOnline,
    streamError,
    openDrawer,
    closeDrawer,
    minimizeDrawer,
    sendMessage,
    startNewConversation
  };
};
