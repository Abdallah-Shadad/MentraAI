"use client";
//components
import { Button } from "@/components/ui/button";
import { motion } from "framer-motion";

//icons
import { Play } from "lucide-react";
import { BackgroundNodes } from "../../../../../components/reusable/BackgroundNodes";
import { Link } from "@/lib/i18n/navigation";

export default function HeroSection() {
  return (
    <section className="main-container relative overflow-hidden min-h-screen">
      <BackgroundNodes />
      {/* Content */}
      <div className="text-center mx-auto flex flex-col items-center absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 w-full z-1">
        <div>
          <motion.div
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5 }}
            className="flex items-center justify-center gap-3 mb-4"
          >
            <img src="/Logo/mentra-mark.svg" alt="MentraAI Mark" className="h-12 sm:h-16 w-auto object-contain" />
            <h1 className="text-3xl sm:text-5xl md:text-4xl lg:text-5xl font-bold bg-linear-to-br from-primary to-secondary bg-clip-text text-transparent">
              MentrAi
            </h1>
          </motion.div>
          <motion.h1
            initial={{ opacity: 0, y: 20 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.5, delay: 0.2 }}
            className="mb-6 text-3xl sm:text-5xl md:text-4xl lg:text-5xl font-bold leading-snug text-foreground"
          >
            Your Smart Learning Journey Starts Here ,<br />
            Guides You Step-by-Step
          </motion.h1>
        </div>
        <motion.p
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.2 }}
          className="mb-10 max-w-xl text-sm leading-7 lg:text-lg text-foreground-muted"
        >
          Unlock your potential with personalized, AI-driven learning paths
          tailored just for you.
        </motion.p>

        <motion.div
          initial={{ opacity: 0, y: 20 }}
          animate={{ opacity: 1, y: 0 }}
          transition={{ duration: 0.5, delay: 0.4 }}
          className="flex flex-col sm:flex-row gap-4"
        >
          <Link href="/register/SignUp">
            <Button
              size="lg"
              className="rounded-xl gradient-cta text-foreground px-8 py-6 text-lg hover:scale-105 transition-all duration-300 cursor-pointer"
            >
              Start Learning
            </Button>
          </Link>
          <Button
            size="lg"
            variant="outline"
            className="flex items-center justify-center rounded-xl border border-border px-8 py-6 text-lg text-foreground hover:scale-105 hover:border-primary hover:bg-primary/40 transition-all duration-300 cursor-pointer"
          >
            <Play size={18} className="mr-2" />
            Watch Demo
          </Button>
        </motion.div>
      </div>

      {/* AI Mentor Image */}
      <div className="absolute top-1/2 left-1/2 -translate-x-1/2 -translate-y-1/2 z-0">
        {/* Glow effects behind image */}
        <div className="inset-0 flex items-center justify-center -z-10">
          <div className="w-96 h-96 bg-secondary/30 rounded-full blur-[100px] animate-pulse" />
          <div
            className="absolute w-72 h-72 bg-surface/25 rounded-full blur-[80px] animate-pulse"
            style={{ animationDelay: "1s" }}
          />
        </div>
      </div>
    </section>
  );
}
