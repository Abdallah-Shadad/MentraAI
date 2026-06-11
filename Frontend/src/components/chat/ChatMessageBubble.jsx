"use client";

import React, { Component } from "react";
import ReactMarkdown from "react-markdown";
import remarkGfm from "remark-gfm";

// Error boundary to protect the layout from malformed markdown syntax streamed during live tokens
class MarkdownErrorBoundary extends Component {
  constructor(props) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError() {
    return { hasError: true };
  }

  componentDidCatch(error, errorInfo) {
    console.error("ReactMarkdown rendering caught an exception:", error, errorInfo);
  }

  render() {
    if (this.state.hasError) {
      // Gracefully fall back to rendering raw text with preserved spacing
      return (
        <div className="whitespace-pre-wrap break-words font-sans text-sm text-current">
          {this.props.fallbackText}
        </div>
      );
    }
    return this.props.children;
  }
}

export default function ChatMessageBubble({ message }) {
  const isUser = message.role === "user";

  return (
    <div
      className={`flex w-full ${isUser ? "justify-end" : "justify-start"} animate-slide-up`}
    >
      <div
        className={`
          max-w-[85%] px-4 py-3 rounded-2xl text-sm leading-relaxed shadow-sm
          ${
            isUser
              ? "bg-primary text-white rounded-br-sm"
              : "bg-surface-elevated text-foreground border border-border rounded-bl-sm"
          }
        `}
      >
        <div className="prose prose-sm dark:prose-invert max-w-none break-words">
          <MarkdownErrorBoundary fallbackText={message.content}>
            <ReactMarkdown 
              remarkPlugins={[remarkGfm]}
              components={{
                // Custom rendering overrides for styling inside chatbot bubble
                p: ({ node, ...props }) => <p className="mb-2 last:mb-0" {...props} />,
                ul: ({ node, ...props }) => <ul className="list-disc pl-4 mb-2 last:mb-0 space-y-1" {...props} />,
                ol: ({ node, ...props }) => <ol className="list-decimal pl-4 mb-2 last:mb-0 space-y-1" {...props} />,
                li: ({ node, ...props }) => <li className="pl-0.5" {...props} />,
                code: ({ node, inline, className, children, ...props }) => {
                  return (
                    <code
                      className={`${
                        isUser ? "bg-white/20 text-white" : "bg-card border border-border text-foreground"
                      } px-1.5 py-0.5 rounded-sm text-xs font-mono`}
                      {...props}
                    >
                      {children}
                    </code>
                  );
                },
                pre: ({ node, ...props }) => (
                  <pre
                    className="bg-card border border-border rounded-md p-3 overflow-x-auto my-2 text-xs font-mono text-foreground"
                    {...props}
                  />
                ),
                a: ({ node, ...props }) => (
                  <a
                    className={`${isUser ? "text-white underline" : "text-secondary hover:underline"} font-medium`}
                    target="_blank"
                    rel="noopener noreferrer"
                    {...props}
                  />
                ),
                table: ({ node, ...props }) => (
                  <div className="overflow-x-auto my-2">
                    <table className="min-w-full border-collapse border border-border text-xs" {...props} />
                  </div>
                ),
                th: ({ node, ...props }) => <th className="border border-border p-1.5 bg-muted font-semibold" {...props} />,
                td: ({ node, ...props }) => <td className="border border-border p-1.5" {...props} />
              }}
            >
              {message.content}
            </ReactMarkdown>
          </MarkdownErrorBoundary>

          {/* Render active cursor block while streaming token chunks */}
          {message.isStreaming && (
            <span className="inline-block w-1.5 h-4 ml-1 bg-current animate-pulse align-middle" />
          )}
        </div>
      </div>
    </div>
  );
}
