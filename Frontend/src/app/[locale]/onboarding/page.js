"use client";
import { motion } from "framer-motion";
import Onboarding from "./_sections/Onboarding";
import { BackgroundNodes } from "@/components/reusable/BackgroundNodes";

export default function OnboardingPage() {
  return (
    <main className="min-h-screen py-12 flex flex-col pt-12 items-center justify-center bg-background gap-8 relative overflow-hidden">
      <BackgroundNodes />
      <div className="main-container w-full max-w-3xl mx-auto px-4">
        {/* Logo */}
        <div className="flex w-fit mx-auto my-2 justify-center items-center">
          <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain dark:hidden" />
          <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain hidden dark:block" />
        </div>

        {/* Header */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -20 }}
          transition={{ duration: 0.5 }}
          className="text-center mb-6"
        >
          <h1 className="text-xl md:text-2xl lg:text-4xl font-bold flex flex-col gap-2 text-foreground mb-2">
            <span>Not sure where to start?</span>
            <span className="primary-gradient">
              We'll help you figure it out.
            </span>
          </h1>
          <p className="text-sm md:text-base text-foreground-muted">
            Let's start your personalized learning journey. Answer a few questions to help us know you.
          </p>
        </motion.div>

        {/* Wizard Container */}
        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          exit={{ opacity: 0, y: -20 }}
          transition={{ duration: 0.5 }}
          className="animate-scale-in"
        >
          <Onboarding />
        </motion.div>
      </div>
    </main>
  );
}
