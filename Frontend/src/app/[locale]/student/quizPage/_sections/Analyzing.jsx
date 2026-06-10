"use client";
import { Brain } from "lucide-react";
import { motion } from "framer-motion";

export default function Analyzing({ step }) {
  const lines = [
    "Analyzing learning patterns…",
    "Detecting weak concepts…",
    "Mapping comprehension graph…",
    "Preparing personalized feedback…",
  ];
  return (
    <div className="mt-20 grid place-items-center text-center animate-fade-in">
      <div className="relative size-48">
        <div className="absolute inset-0 rounded-full border border-primary/30 animate-pulse" />
        <div
          className="absolute inset-4 rounded-full border border-primary/60 animate-pulse"
          style={{ animationDelay: "0.4s" }}
        />
        <div
          className="absolute inset-8 rounded-full border border-primary animate-pulse"
          style={{ animationDelay: "0.8s" }}
        />
        <div className="absolute inset-0 grid place-items-center rounded-full">
          <div className="size-16 rounded-full gradient-cta grid place-items-center shadow-neon">
            <Brain className="size-8 text-white" />
          </div>
        </div>
        {[0, 1, 2].map((i) => (
          <motion.span
            key={i}
            animate={{
              rotate: 360,
            }}
            transition={{
              duration: 3 + i,
              repeat: Infinity,
              ease: "linear",
            }}
            style={{
              // This pushes the dot out from the center
              x: 40 + i * 20,
              // Ensures it rotates around the container's center, not itself
              originX: "calc(-50% - " + (40 + i * 20) + "px)",
            }}
            className="absolute top-1/2 left-1/2 size-2 rounded-full gradient-cta shadow-neon"
          />
        ))}
      </div>
      <h2 className="mt-10 text-2xl font-semibold text-foreground">
        Reading how you think
      </h2>
      <div className="mt-4 space-y-2 max-w-sm mx-auto">
        {lines.map((l, i) => (
          <div
            key={l}
            className={`flex items-center gap-3 text-sm transition-all ${
              i <= step
                ? "text-foreground opacity-100"
                : "text-foreground-muted opacity-40"
            }`}
          >
            <span
              className={`size-1.5 rounded-full ${
                i < step
                  ? "bg-success"
                  : i === step
                    ? "bg-surface animate-pulse"
                    : "bg-muted"
              }`}
            />
            {l}
          </div>
        ))}
      </div>
    </div>
  );
}
