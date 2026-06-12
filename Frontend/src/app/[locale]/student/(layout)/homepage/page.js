"use client";
import { useState } from "react";
import { Link } from "@/lib/i18n/navigation";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";

//components
import AIMentorPanel from "./_sections/AIMentorPanel";
import CurrentFocusSection from "./_sections/CurrentFocusSection";
import TracksRecommendationList from "./_sections/TracksRecommendationList";
import Hero from "./_components/Hero";
import TracksList from "./_sections/TracksList";
import DecisionCard from "./_components/DecisionCard";

//hooks
import { useGetUserProfile } from "@/hooks/useUser";
import { useCareerTrack } from "@/hooks/useCareerTrack";
import { useGetCurrentRoadmap } from "@/hooks/useRoadmap";

//icons
import { Code, Sparkles, Route } from "lucide-react";

export default function HomePage() {
  const [selectedPath, setSelectedPath] = useState(null);

  // Fetch live queries
  const { data: userProfile } = useGetUserProfile();
  const { data: trackQueryData } = useCareerTrack();
  const { data: roadmapQueryData } = useGetCurrentRoadmap();

  const track = trackQueryData?.data;
  const hasTrackSelected = track && track.careerTrackId;

  return (
    <div className="min-h-screen bg-background relative pb-20 text-foreground">
      {/* Main */}
      <main className="main-container">
        <Hero />
        
        <div className="grid grid-cols-1 lg:grid-cols-12 gap-8 mt-6">
          {/* Left / Main content column */}
          <div className="lg:col-span-8 space-y-8">
            
            {/* If track is selected, render active path card */}
            {hasTrackSelected ? (
              <div className="rounded-2xl border border-border bg-card p-6 shadow-sm">
                <div className="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                  <div>
                    <Badge className="bg-primary/10 text-primary border border-primary/20 mb-2">
                      Active Career Path
                    </Badge>
                    <h2 className="text-2xl font-bold text-foreground">
                      {track.careerTrackName}
                    </h2>
                    <p className="text-sm text-foreground-muted mt-1">
                      {track.hasRoadmap 
                        ? "Your learning curriculum is generated and active." 
                        : "Generate your custom AI roadmap to begin your path."}
                    </p>
                  </div>
                  <div className="flex gap-3 shrink-0">
                    <Link href="/student/roadmap">
                      <Button className="bg-primary hover:bg-primary/95 text-white font-semibold cursor-pointer">
                        Go to Roadmap
                      </Button>
                    </Link>
                    <Button 
                      variant="outline"
                      onClick={() => setSelectedPath("not-recommend")}
                      className="border-border hover:bg-muted/50 cursor-pointer text-foreground"
                    >
                      Change Track
                    </Button>
                  </div>
                </div>
              </div>
            ) : (
              /* If no track selected, render decision selector */
              <div className="my-4">
                <div className="mb-6">
                  <h2 className="text-xl md:text-2xl font-semibold text-foreground flex items-center gap-2">
                    <Route className="text-primary" />
                    Choose Your Learning Path
                  </h2>
                  <p className="text-sm text-foreground-muted mt-1">
                    Select a specialization track to start building your career roadmap.
                  </p>
                </div>
                
                <div className="flex flex-col md:flex-row items-stretch gap-4">
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
                    ctaText="Explore Recommended Paths"
                    isPrimary={true}
                    icon={<Sparkles className="w-6 h-6 text-foreground" />}
                    onClick={() => {
                      setSelectedPath("recommend");
                    }}
                  />
                </div>
              </div>
            )}

            {/* Dialog trigger sections */}
            {selectedPath === "not-recommend" && (
              <TracksList
                isOpen={true}
                onOpenChange={(open) => {
                  if (!open) setSelectedPath(null);
                }}
              />
            )}

            {selectedPath === "recommend" && (
              <TracksRecommendationList
                isOpen={true}
                onOpenChange={(open) => {
                  if (!open) setSelectedPath(null);
                }}
              />
            )}
          </div>

          {/* Right / AI Mentor Column */}
          <div className="lg:col-span-4">
            <AIMentorPanel
              userProfile={userProfile}
              trackData={trackQueryData}
              roadmapData={roadmapQueryData}
            />
          </div>
        </div>
      </main>
    </div>
  );
}
