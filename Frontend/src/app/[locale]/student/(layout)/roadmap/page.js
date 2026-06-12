"use client";
import ModuleCard from "./_components/ModuleCard";
import ErrorState from "@/components/reusable/ErrorState";
import { Map, Loader2, Sparkles, Route } from "lucide-react";

// hooks
import { useGenerateRoadmap, useGetCurrentRoadmap } from "@/hooks/useRoadmap";

export default function RoadmapPage() {
  // Fetch the current roadmap on mount
  const {
    data: currentRoadmapData,
    isLoading: isCurrentRoadmapLoading,
    isError: isCurrentRoadmapError,
    refetch: refetchRoadmap,
  } = useGetCurrentRoadmap();

  // Generate roadmap mutation
  const {
    mutate: generateRoadmap,
    isPending: isGenerateRoadmapPending,
    isError: isGenerateRoadmapError,
    error: generateError,
    reset: resetGenerate,
  } = useGenerateRoadmap({
    onSuccess: () => {
      refetchRoadmap();
    },
  });

  const roadmapData = currentRoadmapData?.data?.stages;
  const hasRoadmap = roadmapData && roadmapData.length > 0;

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

        {/* Loading State */}
        {isCurrentRoadmapLoading && (
          <div className="flex flex-col items-center justify-center py-24 gap-4">
            <div className="relative">
              <div className="w-16 h-16 rounded-full bg-primary/10 flex items-center justify-center">
                <Route className="w-8 h-8 text-primary" />
              </div>
              <Loader2 className="w-6 h-6 text-primary animate-spin absolute -top-1 -right-1" />
            </div>
            <p className="text-foreground-muted text-sm font-medium">Loading your roadmap...</p>
          </div>
        )}

        {/* Generate Error */}
        {isGenerateRoadmapError && (
          <ErrorState
            message={generateError?.response?.data?.error?.message || "Failed to generate roadmap"}
            close={() => resetGenerate()}
          />
        )}

        {/* No Roadmap — Show Generate Button */}
        {!isCurrentRoadmapLoading && !hasRoadmap && (
          <div className="flex flex-col items-center justify-center py-24 gap-6">
            <div className="w-20 h-20 rounded-2xl bg-primary/10 border border-primary/20 flex items-center justify-center">
              <Route className="w-10 h-10 text-primary" />
            </div>
            <div className="text-center max-w-sm">
              <h3 className="text-xl font-bold text-foreground mb-2">No Roadmap Yet</h3>
              <p className="text-foreground-muted text-sm leading-relaxed">
                Generate a personalized AI-powered roadmap based on your selected career track.
              </p>
            </div>
            <button
              onClick={() => generateRoadmap()}
              disabled={isGenerateRoadmapPending}
              className="flex items-center gap-2 px-6 py-3 rounded-lg bg-primary/20 border border-primary hover:bg-primary text-foreground font-semibold shadow-shadow-neon transition-all active:scale-[0.98] cursor-pointer disabled:opacity-60 disabled:cursor-not-allowed"
            >
              {isGenerateRoadmapPending ? (
                <>
                  <Loader2 className="w-4 h-4 animate-spin" />
                  Generating Roadmap...
                </>
              ) : (
                <>
                  <Sparkles className="w-4 h-4" />
                  Generate Your Roadmap
                </>
              )}
            </button>
          </div>
        )}

        {/* Roadmap Modules */}
        {hasRoadmap && (
          <div className="space-y-2 w-full mt-10">
            {roadmapData.map((module, index) => (
              <ModuleCard
                key={module.stageProgressId}
                id={module.stageProgressId}
                title={module.stageName}
                lessonCount={"unknown"}
                difficulty={"unknown"}
                status={module.status}
                aiTooltip={"unknown"}
                index={index}
                totalModules={roadmapData.length}
              />
            ))}
          </div>
        )}
      </section>
    </div>
  );
}
