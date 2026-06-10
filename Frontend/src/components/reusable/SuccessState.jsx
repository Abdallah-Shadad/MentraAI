"use client";

import { motion } from "framer-motion";
import { Check, X } from "lucide-react";

export default function SuccessState({ message, close, icon, className = "" }) {
  return (
    <motion.div
      initial={{ opacity: 0, scale: 0.95, y: 20 }}
      animate={{ opacity: 1, scale: 1, y: 0 }}
      transition={{
        duration: 0.35,
        ease: [0.22, 1, 0.36, 1],
      }}
      className={`fixed top-1/2 left-1/2 z-50 w-[90%] max-w-md -translate-x-1/2 -translate-y-1/2 overflow-hidden rounded-2xl border border-emerald-500/20 card/90 backdrop-blur-xl shadow-2xl shadow-emerald-500/10 ${className}`}
    >
      {/* Glow Background */}
      <div className="absolute inset-0 bg-linear-to-br from-emerald-500/5 via-transparent to-primary/5 pointer-events-none" />

      {/* Close Button */}
      <button
        onClick={close}
        className="absolute right-3 top-3 z-10 flex h-9 w-9 items-center justify-center rounded-full border border-border/50 bg-background/40 text-foreground transition-all duration-200 hover:border-emerald-500/30 hover:bg-emerald-500/10 hover:text-emerald-400"
      >
        <X className="h-4 w-4" />
      </button>

      <div className="relative flex flex-col items-center px-6 py-8 text-center">
        {/* Icon */}
        <motion.div
          initial={{ scale: 0, rotate: -180 }}
          animate={{ scale: 1, rotate: 0 }}
          transition={{
            type: "spring",
            stiffness: 220,
            damping: 16,
            delay: 0.1,
          }}
          className="relative mb-6"
        >
          {/* Glow */}
          <div className="absolute inset-0 scale-125 rounded-full bg-emerald-500/20 blur-xl" />

          {/* Circle */}
          <div className="relative flex h-16 w-16 items-center justify-center rounded-full border border-emerald-500/20 bg-gradient-to-br from-emerald-500/20 to-emerald-500/5 shadow-[0_0_30px_rgba(16,185,129,0.25)]">
            {icon || (
              <Check className="h-8 w-8 text-emerald-400" strokeWidth={2.5} />
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
          Success
        </motion.h3>

        {/* Message */}
        {message && (
          <motion.p
            initial={{ opacity: 0, y: 10 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ delay: 0.3 }}
            className="max-w-sm text-sm leading-relaxed text-foreground md:text-base wrap-break-word"
          >
            {message}
          </motion.p>
        )}
      </div>

      {/* Bottom Neon Line */}
      <div className="h-px w-full bg-linear-to-r from-transparent via-emerald-500/40 to-transparent" />
    </motion.div>
  );
}
