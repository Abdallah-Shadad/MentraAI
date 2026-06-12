"use client";
import { cn } from "@/lib/utils";
//icons
import { ArrowRight, Briefcase } from "lucide-react";
import { useRouter } from "@/lib/i18n/navigation";

export const TrackCard = ({
  Id,
  Name,
  icon,
  careers,
  Description,
  postTrackSelection,
}) => {
  const router = useRouter();
  const handleTrackSelection = async () => {
    await postTrackSelection(Id);
    router.push("/student/roadmap");
  };

  return (
    <div
      className={cn(
        "group relative rounded-xl p-4 cursor-pointer",
        "bg-card border border-border",
        "transition-all duration-300",
        "hover:border-primary/50 hover:glow-primary hover:-translate-y-1 z-10",
      )}
    >
      {/* Icon */}
      <div className="w-12 h-12 rounded-xl bg-linear-to-br from-primary/20 to-secondary/10 flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
        {icon}
      </div>

      {/* Track Name */}
      <h4 className="text-lg font-semibold text-foreground mb-2">{Name}</h4>

      <p className="text-sm text-foreground-muted mb-4">{Description}</p>

      {/* Career Badge */}
      <div className="flex flex-wrap gap-2 mb-4">
        {careers.slice(0, 3).map((career, idx) => (
          <span
            key={idx}
            className="px-2 py-1 text-xs rounded-full bg-secondary/10 text-secondary border border-secondary/20 flex items-center gap-1"
          >
            <Briefcase className="w-3 h-3" />
            {career}
          </span>
        ))}
      </div>

      <button
        onClick={handleTrackSelection}
        className="group w-full text-center flex items-center justify-center gap-2 text-sm text-primary bg-primary/10 hover:bg-primary hover:text-muted hover:dark:text-foreground transition-colors rounded-sm px-3 py-2"
      >
        <span>Choose Track</span>
        <ArrowRight className="w-4 h-4 text-foreground group-hover:translate-x-1 transition-transform" />
      </button>
    </div>
  );
};
