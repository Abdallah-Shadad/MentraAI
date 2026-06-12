"use client";
import { useState } from "react";

//components
import AIMentorPanel from "./_sections/AIMentorPanel";
import CurrentFocusSection from "./_sections/CurrentFocusSection";
import TracksRecommendationList from "./_sections/TracksRecommendationList";
import Hero from "./_components/Hero";
import TracksList from "./_sections/TracksList";
import DecisionCard from "./_components/DecisionCard";
//icons
import { Code, Sparkles, Route } from "lucide-react";

export default function HomePage() {
  const [selectedPath, setSelectedPath] = useState(null);
  return (
    <div className="min-h-screen bg-background relative pb-20">
      {/* Main */}
      <main className="main-container">
        <Hero />
        <div>
          {/* Main Content */}
          {/* Current Focus */}
          {/* <CurrentFocusSection
            moduleName="React Foundations"
            progress={60}
            lastLesson="Component Lifecycle"
            totalLessons={15}
            completedLessons={9}
            weakPoint="State Management"
          /> */}

          <div>
            {/* Choose Your Learning Path */}
            <div className="my-8">
              <h2 className="text-xl md:text-3xl font-semibold text-foreground mb-8">
                <span className="flex items-center gap-2">
                  <Route />
                  Choose Your Learning Path?
                </span>
              </h2>
              <div className="flex flex-col md:flex-row items-center gap-4">
                <DecisionCard
                  title="I know what I want to learn."
                  description="Choose a learning track you already have in mind and start immediately."
                  ctaText="Choose Path"
                  isPrimary={false}
                  icon={<Code className="w-6 h-6 text-foreground" />}
                  onClick={() => setSelectedPath("not-recommend")}
                />
                <DecisionCard
                  title="Analyze Knowledge Gaps"
                  description="Let our AI analyze your background and recommend the highest-impact learning path."
                  ctaText="Explore Alternative Paths"
                  isPrimary={true}
                  icon={<Sparkles className="w-6 h-6 text-foreground" />}
                  onClick={() => {
                    setSelectedPath("recommend");
                  }}
                />
              </div>
            </div>

            {selectedPath == "not-recommend" && (
              <TracksList
                isOpen={true}
                onOpenChange={() => {
                  setSelectedPath(null);
                }}
              />
            )}

            {selectedPath == "recommend" && (
              <TracksRecommendationList
                isOpen={true}
                onOpenChange={() => {
                  setSelectedPath(null);
                }}
              />
            )}
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
    </div>
  );
}
