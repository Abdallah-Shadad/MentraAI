"use client";
import { motion } from "framer-motion";
import { Link } from "@/lib/i18n/navigation";
import SignupForm from "./_sections/SignupForm";
import { BackgroundNodes } from "@/components/reusable/BackgroundNodes";
import Header from "./_sections/Header";
import { Home } from "lucide-react";

export default function RegisterPage() {
  return (
    <main className="min-h-screen py-12 flex flex-col pt-12 items-center justify-center bg-background gap-8 relative overflow-hidden">
      <BackgroundNodes />
      <div className="main-container">
        {/* Logo */}
        <div className="flex w-fit mx-auto my-2 justify-center items-center">
          <img src="/Logo/mentra-logo-light.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain dark:hidden" />
          <img src="/Logo/mentra-logo-dark.svg" alt="MentraAI Logo" className="h-8 w-auto object-contain hidden dark:block" />
        </div>

        {/* Header */}
        <Header />

        <motion.div
          initial={{ opacity: 0, x: 20 }}
          animate={{ opacity: 1, x: 0 }}
          exit={{ opacity: 0, x: -20 }}
          transition={{ duration: 0.5 }}
          className="w-full animate-scale-in"
        >
          <div className="w-full max-w-md mx-auto shrink-0 mb-6">
            <Link
              href="/"
              className="cursor-pointer mb-2 py-1 flex items-center gap-2 text-foreground-muted font-semibold hover:text-foreground disabled:opacity-50 mr-auto"
            >
              <Home className="w-5 h-5" /> Home
            </Link>
            <SignupForm />
          </div>
        </motion.div>
      </div>
    </main>
  );
}
