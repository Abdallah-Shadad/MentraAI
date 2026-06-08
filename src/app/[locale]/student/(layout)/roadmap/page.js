"use client";
import ModuleCard from "./_components/ModuleCard";
import TrackHeader from "./_components/TrackHeader";
import ErrorState from "@/components/reusable/ErrorState";

import { Map } from "lucide-react";
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

//hooks
import { useGenerateRoadmap, useGetCurrentRoadmap } from "@/hooks/useRoadmap";
import { useEffect } from "react";

export default function RoadmapPage() {
  // Generate roadmap
  const {
    mutate: generateRoadmap,
    isPending: isGenerateRoadmapPending,
    isError: isGenerateRoadmapError,
    error,
    data,
  } = useGenerateRoadmap();
  // Get current roadmap
  const {
    queryFn: getCurrentRoadmap,
    data: currentRoadmapData,
    isLoading: isCurrentRoadmapLoading,
    isError: isCurrentRoadmapError,
  } = useGetCurrentRoadmap();

  // Roadmap data
  const roadmapData = data?.data?.stages || currentRoadmapData?.data?.stages;

  useEffect(() => {
    generateRoadmap();
    if (isGenerateRoadmapError) {
      getCurrentRoadmap();
    }
  }, [generateRoadmap]);

  return (
    <div className="min-h-screen relative py-8 bg-background">
      <section className="main-container">
        <div className="mb-6">
          <h2 className="font-heading font-semibold text-3xl text-foreground mb-2 flex items-center">
            <Map className="inline-block mr-2" />
            Your Learning Journey
          </h2>
          <p className="text-foreground-muted text-sm">
            Track your progress and stay motivated on your learning path.
          </p>
        </div>

        {(isGenerateRoadmapPending || isCurrentRoadmapLoading) && (
          <div className="flex items-center justify-center py-10">
            <div>Loading...</div>
          </div>
        )}

        <TrackHeader
          title="Frontend Developer – Beginner to Job Ready"
          description="Master modern frontend development with React, TypeScript, and industry best practices."
          progress={35}
          estimatedWeeks={8}
          totalModules={7}
          completedModules={2}
          totalLessons={77}
          completedLessons={18}
        />

        <div className="space-y-2 w-full mt-10">
          {roadmapData?.map((module, index) => (
            <ModuleCard
              key={module.stageProgressId}
              id={module.stageProgressId}
              title={module.stageName}
              lessonCount={"unknown"}
              difficulty={"unknown"}
              status={module.status}
              aiTooltip={"unknown"}
              index={index}
              totalModules={"unknown"}
            />
          ))}
        </div>
      </section>
    </div>
  );
}
