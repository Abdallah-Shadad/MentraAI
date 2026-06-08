"use client";
import { useState } from "react";
import { motion } from "framer-motion";
import { Link } from "@/lib/i18n/navigation";
//comment
import SignupForm from "./_sections/SignupForm";
import TracksList from "../../student/(layout)/homepage/_sections/TracksList";
import Onboarding from "./_sections/Onboarding";
import { BackgroundNodes } from "@/components/reusable/BackgroundNodes";
import Header from "./_sections/Header";
//icons
import { Home, ArrowLeft } from "lucide-react";

export default function RegisterPage() {
  const [steps, setSteps] = useState(1);
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
                className="cursor-pointer mb-2 py-1 flex items-center gap-2 text-foreground-muted font-semibold hover:text-foreground disabled:opacity-50 mr-auto"
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
              className="group cursor-pointer mb-6 py-3 text-foreground-muted font-semibold hover:text-foreground disabled:opacity-50 mr-auto"
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
              className="animate-scale-in"
            >
              <Onboarding />
            </motion.div>
          </div>
        )}

        {steps === 3 && selectedOption === "option1" && (
          <div className="w-full max-w-4xl mx-auto mb-6 min-h-screen">
            <button
              onClick={() => setSteps(steps - 1)}
              className="group cursor-pointer mb-4 py-3 text-foreground-muted font-semibold hover:text-foreground disabled:opacity-50 mr-auto"
            >
              <span>
                <ArrowLeft className="inline mx-1 w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                Back
              </span>
            </button>
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-foreground mb-2">
                Choose Your Track
              </h2>
              <p className="text-foreground-muted">
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
              className="group cursor-pointer mb-4 py-3 text-foreground-muted font-semibold hover:text-foreground disabled:opacity-50 mr-auto"
            >
              <span>
                <ArrowLeft className="inline mx-1 w-5 h-5 group-hover:-translate-x-1 transition-transform" />
                Back
              </span>
            </button>
            <div className="mb-6">
              <h2 className="text-2xl font-bold text-foreground mb-2">
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
