"use client";
import { motion } from "framer-motion";

export default function ProgressBar({ progress, answered, total }) {
  /**
   * progress: number - percentage of progress
   * answered: number - number of answered questions
   * total: number - total number of questions
   */

  return (
    <div className="mb-4 border border-primary bg-surface/5 backdrop-blur-lg rounded-3xl p-5">
      <div className="flex flex-wrap items-center justify-between gap-3">
        <div className="text-sm text-foreground-secondary">
          <span className="text-foreground font-semibold">{answered}</span> of{" "}
          {total} questions explored
        </div>
      </div>
      <div className="mt-3 h-2 rounded-full bg-surface/20 overflow-hidden relative">
        <motion.div
          initial={{ width: 0 }}
          animate={{ width: `${progress}%` }}
          transition={{ duration: 0.4, ease: "easeOut" }}
          className="absolute top-0 bottom-0 left-0 bg-gradient-to-r from-primary to-secondary"
        />
      </div>
    </div>
  );
}
