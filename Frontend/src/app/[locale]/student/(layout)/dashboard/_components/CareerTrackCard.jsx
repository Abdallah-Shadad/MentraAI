"use client";
import { Award, Navigation, Settings, ArrowRight } from "lucide-react";
import { useRouter } from "@/lib/i18n/navigation";

export default function CareerTrackCard({ trackData, isError }) {
  const router = useRouter();

  const handleRouteToHomepage = () => {
    router.push("/student");
  };

  const handleRouteToRoadmap = () => {
    router.push("/student/roadmap");
  };

  // If no track chosen (either error state, 404, or trackData is null/undefined)
  const isNoTrack = isError || !trackData;

  if (isNoTrack) {
    return (
      <div className="bg-card/50 backdrop-blur-sm border border-border rounded-lg p-6 shadow-shadow-card">
        <div className="flex items-center gap-3 mb-4">
          <Award className="w-6 h-6 text-foreground-muted" />
          <h4 className="text-xl font-bold text-foreground">Active Career Track</h4>
        </div>
        <p className="text-sm text-foreground-muted mb-6">
          No career track has been selected yet. Take the first step towards your career goals by choosing one of our curated tracks.
        </p>
        <button
          onClick={handleRouteToHomepage}
          className="flex items-center justify-center gap-2 px-5 py-3 bg-primary/20 border border-primary/40 hover:bg-primary text-foreground font-bold rounded-md shadow-shadow-neon transition-all active:scale-[0.98] cursor-pointer w-full sm:w-auto"
        >
          <span>Select Learning Track</span>
          <ArrowRight className="w-4 h-4" />
        </button>
      </div>
    );
  }

  const { careerTrackName, slug, selectionType, selectedAt, hasRoadmap } = trackData;

  const formattedSelectedDate = selectedAt
    ? new Date(selectedAt).toLocaleDateString(undefined, {
        year: "numeric",
        month: "short",
        day: "numeric",
      })
    : "Recently";

  return (
    <div className="bg-card/50 backdrop-blur-sm border border-border rounded-lg p-6 shadow-shadow-card relative overflow-hidden">
      {/* Glow */}
      <div className="absolute -bottom-10 -left-10 w-24 h-24 bg-secondary/10 blur-xl rounded-full pointer-events-none"></div>

      <div className="flex flex-col gap-6">
        <div className="flex items-center justify-between gap-4">
          <div className="flex items-center gap-3">
            <Award className="w-6 h-6 text-primary" />
            <h4 className="text-xl font-bold text-foreground">Active Career Track</h4>
          </div>

          <button
            onClick={handleRouteToHomepage}
            className="flex items-center gap-1.5 text-xs font-semibold px-3 py-1.5 border border-border hover:border-foreground-muted/50 rounded-md text-foreground transition-all cursor-pointer bg-surface-elevated/40"
          >
            <Settings className="w-3.5 h-3.5" />
            <span>Change Track</span>
          </button>
        </div>

        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 p-4 rounded-lg bg-surface-elevated/20 border border-border/40">
          <div>
            <div className="flex flex-wrap items-center gap-2 mb-1">
              <span className="text-lg font-bold text-foreground">
                {careerTrackName}
              </span>
              <span className="text-[10px] font-bold px-2 py-0.5 rounded-full bg-secondary/20 text-secondary border border-secondary/30 uppercase tracking-wider">
                {selectionType}
              </span>
            </div>
            <p className="text-xs text-foreground-muted">
              Selected on: {formattedSelectedDate} • Slug: <code className="text-foreground-light bg-black/30 px-1 py-0.5 rounded-xs text-[10px]">{slug}</code>
            </p>
          </div>

          <button
            onClick={handleRouteToRoadmap}
            className="flex items-center gap-2 px-5 py-2.5 bg-primary/20 border border-primary hover:bg-primary text-foreground font-bold rounded-md shadow-shadow-neon transition-all active:scale-[0.98] cursor-pointer shrink-0"
          >
            <Navigation className="w-4 h-4" />
            <span>{hasRoadmap ? "View Roadmap" : "Generate Roadmap"}</span>
          </button>
        </div>
      </div>
    </div>
  );
}
