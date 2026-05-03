"use client";

import TrackHeader from "./_sections/TrackHeader";
import AIMentorPanel from "./_sections/AIMentorPanel";
import ModuleCard from "./_sections/ModuleCard";
import CurrentFocusSection from "./_sections/CurrentFocusSection";
import TopBar from "../_components/TopBar";

// Mock data
const mockModules = [
  {
    id: "1",
    title: "HTML & CSS Fundamentals",
    lessonCount: 12,
    difficulty: "Beginner",
    status: "completed",
  },
  {
    id: "2",
    title: "JavaScript Essentials",
    lessonCount: 18,
    difficulty: "Beginner",
    status: "completed",
  },
  {
    id: "3",
    title: "React Foundations",
    lessonCount: 15,
    difficulty: "Intermediate",
    status: "current",
  },
  {
    id: "4",
    title: "State Management with Redux",
    lessonCount: 10,
    difficulty: "Intermediate",
    status: "needsReview",
  },
  {
    id: "5",
    title: "TypeScript for React",
    lessonCount: 8,
    difficulty: "Intermediate",
    status: "locked",
    aiTooltip: "Complete React Foundations first to unlock this module.",
  },
  {
    id: "6",
    title: "Testing & Best Practices",
    lessonCount: 14,
    difficulty: "Advanced",
    status: "locked",
    aiTooltip:
      "This module requires understanding of TypeScript and React patterns.",
  },
  {
    id: "7",
    title: "Building Real-World Projects",
    lessonCount: 20,
    difficulty: "Advanced",
    status: "locked",
    aiTooltip:
      "You'll need all previous skills to tackle these capstone projects.",
  },
];

const mockNextActions = [
  {
    id: "1",
    type: "continue",
    title: "Continue React Foundations",
    description: "Pick up where you left off in Lesson 8: Component Lifecycle",
    aiReason: "You're 60% through this module with great momentum.",
    priority: "high",
  },
  {
    id: "2",
    type: "review",
    title: "Review State Management",
    description: "Revisit Redux concepts before moving forward",
    aiReason: "Strengthening this will help with upcoming modules.",
    priority: "medium",
  },
  {
    id: "3",
    type: "quiz",
    title: "Take Module Quiz",
    description: "Test your JavaScript knowledge with a quick assessment",
    aiReason: "It's been 2 weeks since you completed this module.",
    priority: "low",
  },
];

export default function HomePage() {
  return (
    <div className="min-h-screen bg-background ">
      {/* Main */}
      <main className="main-container px-4 py-8">
        <TopBar />
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8">
          {/* Main Content */}
          <div className="lg:col-span-8 space-y-8">
            <TrackHeader
              title="Frontend Developer – Beginner to Job Ready"
              description="Master modern frontend development with React, TypeScript, and industry best practices."
              progress={35}
              estimatedWeeks={8}
              totalModules={7}
              completedModules={2}
            />

            {/* Roadmap */}
            <section>
              <h2 className="font-heading font-semibold text-xl text-text-foreground mb-6">
                Your Learning Journey
              </h2>

              <div className="space-y-2">
                {mockModules.map((module, index) => (
                  <ModuleCard
                    key={module.id}
                    {...module}
                    index={index}
                    totalModules={mockModules.length}
                  />
                ))}
              </div>
            </section>

            {/* Current Focus */}
            <CurrentFocusSection
              moduleName="React Foundations"
              progress={60}
              lastLesson="Component Lifecycle"
              totalLessons={15}
              completedLessons={9}
              weakPoint="State Management"
            />
          </div>

          {/* Sidebar */}
          <div className="lg:col-span-4 space-y-6">
            <AIMentorPanel
              userName="John"
              currentModule="React Foundations"
              state="progress"
              recommendation="Focus on React hooks before moving forward."
            />
          </div>
        </div>
      </main>

      {/* Footer */}
      <footer className="border-t border-border mt-16">
        <div className="container mx-auto px-4 py-6">
          <p className="text-sm text-text-muted text-center">
            © 2024 MentraAI. Your AI-powered learning companion.
          </p>
        </div>
      </footer>
    </div>
  );
}
