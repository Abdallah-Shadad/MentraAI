"use client";

import { motion } from "framer-motion";
import { AlertTriangle, X } from "lucide-react";

export default function ErrorState({ message, icon, close, className = "" }) {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95, y: 20 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      transition={{
        duration: 0.35,
        ease: [0.22, 1, 0.36, 1],
      }}
      className={`fixed top-1/2 left-1/2 z-50 w-[90%] max-w-md -translate-x-1/2 -translate-y-1/2 overflow-hidden rounded-2xl border border-red-500/20 bg-card/95 backdrop-blur-xl shadow-2xl shadow-red-500/10 ${className}`}
    >
      {/* Glow Background */}
      <div className="absolute inset-0 bg-linear-to-br from-red-500/5 via-transparent to-primary/5 pointer-events-none" />

      {/* Close Button */}
      <button
        onClick={close}
        className="absolute right-3 top-3 z-10 flex h-9 w-9 items-center justify-center rounded-full border border-border/50 bg-background/40 text-foreground transition-all duration-200 hover:border-red-500/30 hover:bg-red-500/10 hover:text-red-400 cursor-pointer"
      >
        <X className="h-4 w-4" />
      </button>

      <div className="relative flex flex-col items-center px-6 py-8 text-center">
        {/* Icon */}
        <motion.div
          initial={{ scale: 0 }}
          animate={{ scale: 1 }}
          transition={{
            type: "spring",
            stiffness: 220,
            damping: 16,
            delay: 0.1,
          }}
          className="relative mb-6"
        >
          {/* Glow Ring */}
          <div className="absolute inset-0 scale-125 rounded-full bg-red-500/20 blur-xl" />

          {/* Circle */}
          <div className="relative flex h-16 w-16 items-center justify-center rounded-full border border-red-500/20 bg-linear-to-br from-red-500/20 to-red-500/5 shadow-[0_0_30px_rgba(239,68,68,0.25)]">
            {icon || (
              <AlertTriangle
                className="h-8 w-8 text-red-400"
                strokeWidth={2.5}
              />
            )}
          </div>
        </motion.div>

        {/* Title */}
        <motion.h3
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.2 }}
          className="mb-2 text-xl font-semibold text-foreground"
        >
          Error
        </motion.h3>

        {/* Message */}
        <motion.p
          initial={{ opacity: 0, y: 10 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ delay: 0.3 }}
          className="max-w-sm text-base md:text-lg font-semibold leading-relaxed text-foreground wrap-break-word"
        >
          {message}
        </motion.p>
      </div>

      {/* Bottom Neon Line */}
      <div className="h-px w-full bg-linear-to-r from-transparent via-red-500/40 to-transparent" />
    </motion.div>
  );
}
