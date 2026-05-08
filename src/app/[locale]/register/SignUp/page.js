"use client";
import SignupForm from "./_sections/SignupForm";
import DecisionCard from "./_components/DecisionCard";
import TracksList from "./_sections/TracksList";
import { BackgroundNodes } from "../../(mentarAi)/homepage/_components/BackgroundNodes";
import { motion } from "framer-motion";
import { Home, ArrowLeft, Code, Sparkles } from "lucide-react";
import { useState } from "react";
import { Link } from "@/lib/i18n/navigation";
export default function RegisterPage() {
  const [steps, setSteps] = useState(1);
  const [selectedOption, setSelectedOption] = useState(null);
  return (
    <main className="min-h-screen py-12 flex flex-col pt-12 items-center justify-center bg-background gap-8 relative overflow-hidden">
      <BackgroundNodes />
      <div className="main-container ">
        {/* Logo */}
        <div className="flex w-fit mx-auto my-2 justify-center items-center gap-2 px-4 py-1 rounded-full border border-purple-500/40 bg-purple-500/10">
          <div className="w-2.5 h-2.5 bg-purple-500 rounded-full shadow-[0_0_8px_rgba(139,92,246,.7)]" />
          <span className="text-purple-300 text-sm font-semibold">
            MentarAI
          </span>
        </div>

        {/* Header */}
        <Header steps={steps} />

        {steps === 1 && (
          <motion.div
            initial={{ opacity: 0, x: 20 }}
            animate={{ opacity: 1, x: 0 }}
            exit={{ opacity: 0, x: -20 }}
            transition={{ duration: 0.5 }}
            className="w-full"
          >
            <div className="w-full max-w-md mx-auto shrink-0 mb-6">
              <Link
                href="/"
                className="cursor-pointer mb-2 py-1 flex items-center gap-2 text-text-muted font-semibold hover:text-text-foreground disabled:opacity-50 mr-auto"
              >
                <Home className="w-5 h-5" /> Home
              </Link>
              <SignupForm steps={steps} setSteps={setSteps} />
            </div>
          </motion.div>
        )}

        {steps === 2 && (
          <div className="w-full max-w-3xl mx-auto">
            <button
              onClick={() => setSteps(steps - 1)}
              className="group cursor-pointer mb-6 py-3 text-text-muted font-semibold hover:text-text-foreground disabled:opacity-50 mr-auto"
            >
              <span>
                <ArrowLeft className="inline mx-1 w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                Back
              </span>
            </button>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              exit={{ opacity: 0, y: -20 }}
              transition={{ duration: 0.5 }}
              className="grid md:grid-cols-2 gap-6 animate-scale-in "
            >
              <DecisionCard
                title="I know what I want to learn"
                description="Choose a learning track you already have in mind and start immediately."
                ctaText="Explore Tracks"
                tooltip="You can always adjust your path later based on your performance."
                icon={<Code className="w-7 h-7 text-text-foreground" />}
                onClick={() => {
                  setSelectedOption("option1");
                  setSteps(3);
                }}
              />
              <DecisionCard
                title="I'm not sure / I'm confused"
                description="Answer a few simple questions and let AI recommend the best path for you."
                ctaText="Help Me Choose (AI Quiz)"
                isPrimary={true}
                icon={<Sparkles className="w-7 h-7 text-text-accent" />}
                onClick={() => {
                  setSelectedOption("option2");
                  setSteps(3);
                }}
              />
            </motion.div>
          </div>
        )}

        {steps === 3 && selectedOption === "option1" && (
          <div className="w-full max-w-4xl mx-auto mb-6 min-h-screen">
            <button
              onClick={() => setSteps(steps - 1)}
              className="group cursor-pointer mb-4 py-3 text-text-muted font-semibold hover:text-text-foreground disabled:opacity-50 mr-auto"
            >
              <span>
                <ArrowLeft className="inline mx-1 w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                Back
              </span>
            </button>
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-text-foreground mb-2">
                Choose Your Track
              </h2>
              <p className="text-text-muted">
                Select a learning path that aligns with your goals
              </p>
            </div>
            <motion.div
              initial={{ opacity: 0, x: 20 }}
              animate={{ opacity: 1, x: 0 }}
              exit={{ opacity: 0, x: -20 }}
              transition={{ duration: 0.5 }}
            >
              <TracksList />
            </motion.div>{" "}
          </div>
        )}

        {steps === 3 && selectedOption === "option2" && (
          <div className="w-full max-w-4xl mx-auto mb-6 min-h-screen">
            <button
              onClick={() => setSteps(steps - 1)}
              className="group cursor-pointer mb-4 py-3 text-text-muted font-semibold hover:text-text-foreground disabled:opacity-50 mr-auto"
            >
              <span>
                <ArrowLeft className="inline mx-1 w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                Back
              </span>
            </button>
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-text-foreground mb-2">
                AI Quiz
              </h2>
              <div className="border border-destructive rounded-lg p-4 mt-10">
                <p className="text-destructive italic font-bold">
                  Sorry,We are still working on this feature,please check back
                  later.
                </p>
              </div>
            </div>
          </div>
        )}
      </div>
    </main>
  );
}

function Header({ steps }) {
  if (steps === 1) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -20 }}
        transition={{ duration: 0.5 }}
        className="text-center mb-4"
      >
        <h1 className="text-xl md:text-2xl lg:text-4xl font-bold text-white mb-2">
          Student Registration
        </h1>
        <p className="text-sm md:text-base text-text-muted">
          Join the AI-powered learning platform
        </p>
      </motion.div>
    );
  }
  if (steps > 1) {
    return (
      <motion.div
        initial={{ opacity: 0, y: 20 }}
        animate={{ opacity: 1, y: 0 }}
        exit={{ opacity: 0, y: -20 }}
        transition={{ duration: 0.5 }}
        className="text-center mb-4"
      >
        <h1 className="text-xl md:text-2xl lg:text-4xl font-bold flex flex-col gap-2 text-text-primary mb-2">
          <span>Not sure where to start?</span>
          <span className="primary-gradient">
            We'll help you figure it out.
          </span>
        </h1>
        <p className="text-sm md:text-base text-text-muted">
          Choose your own path, or let AI recommend the best learning journey
          for you.
        </p>
      </motion.div>
    );
  }

  return null;
}
