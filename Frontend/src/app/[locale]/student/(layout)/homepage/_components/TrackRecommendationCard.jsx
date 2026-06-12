//icons
import { Brain, CheckCircle2, Sparkles, ArrowUpRight, Loader2 } from "lucide-react";
//hooks
import { useTrackSelection } from "@/hooks/useCareerTrack";
import { useRouter } from "@/lib/i18n/navigation";

export default function TrackRecommendationCard({ track }) {
  const circumference = 2 * Math.PI * 40;
  const offset = circumference - (track.fitScore / 100) * circumference;
  const router = useRouter();

  const {
    mutate: selectTrack,
    isPending,
    isError,
    isSuccess,
    error,
    reset,
  } = useTrackSelection();

  const handleSelectTrack = async () => {
    selectTrack(track.trackId, {
      onSuccess: () => {
        router.push("/student/roadmap");
      },
    });
  };

  return (
    <article className="group min-h-screen overflow-y-auto relative overflow-hidden rounded-lg border border-border bg-card p-6 shadow-shadow-card transition-all duration-300 hover:-translate-y-1 hover:shadow-shadow-soft shrink-0">
      {/* glow */}
      <div className="absolute inset-0 bg-[linear-gradient(135deg,rgba(124,58,237,0.08),transparent_40%)] opacity-0 transition-opacity duration-300 group-hover:opacity-100" />

      <div className="relative z-10">
        {/* header */}
        <div className="flex flex-col md:flex-row items-center gap-2 justify-between">
          <div className="flex items-center gap-3">
            <div className="flex size-12 items-center justify-center rounded-2xl bg-accent text-primary">
              <Brain className="size-6" />
            </div>

            <div>
              <div className="mb-1 flex items-center gap-2">
                <Sparkles className="size-4 text-primary" />

                <span className="text-xs font-medium uppercase tracking-wider text-foreground-muted">
                  Recommended Track
                </span>
              </div>

              <h3 className="text-lg font-bold text-foreground">
                {track.trackName}
              </h3>
            </div>
          </div>

          {/* score ring */}
          <div className="relative size-20 shrink-0">
            <svg className="size-20 -rotate-90" viewBox="0 0 100 100">
              <circle
                cx="50"
                cy="50"
                r="40"
                stroke="var(--border)"
                strokeWidth="8"
                fill="none"
              />

              <circle
                cx="50"
                cy="50"
                r="40"
                stroke="url(#scoreGradient)"
                strokeWidth="8"
                strokeLinecap="round"
                fill="none"
                strokeDasharray={circumference}
                strokeDashoffset={offset}
              />

              <defs>
                <linearGradient
                  id="scoreGradient"
                  x1="0%"
                  y1="0%"
                  x2="100%"
                  y2="100%"
                >
                  <stop offset="0%" stopColor="var(--primary)" />
                  <stop offset="100%" stopColor="var(--secondary)" />
                </linearGradient>
              </defs>
            </svg>

            <div className="absolute inset-0 flex flex-col items-center justify-center">
              <span className="text-xl font-bold text-foreground">
                {track.fitScore}
              </span>

              <span className="text-[10px] text-foreground-muted">MATCH</span>
            </div>
          </div>
        </div>

        {/* reasoning */}
        <div className="mt-5 rounded-xl border border-border bg-surface-elevated p-4">
          <p className="text-sm leading-6 text-foreground-secondary">
            {track.reasoning}
          </p>
        </div>

        {/* skills */}
        <div className="mt-6 grid gap-4 md:grid-cols-2">
          <div>
            <h4 className="mb-3 text-sm font-semibold text-foreground">
              Matching Skills
            </h4>

            <div className="flex flex-wrap gap-2">
              {track.skillOverlap.map((skill, index) => (
                <span
                  key={`${skill}-${index}`}
                  className="inline-flex items-center gap-1 rounded-full border border-success bg-success/10 px-3 py-1.5 text-xs font-medium text-success"
                >
                  <CheckCircle2 className="size-3.5" />
                  {skill}
                </span>
              ))}
            </div>
          </div>

          <div>
            <h4 className="mb-3 text-sm font-semibold text-foreground">
              Skills To Learn
            </h4>

            <div className="flex flex-wrap gap-2">
              {track.skillsToLearn.map((skill, index) => (
                <span
                  key={`${skill}-${index}`}
                  className="rounded-full bg-muted px-4 py-2 text-xs font-medium leading-snug text-foreground-secondary break-keep whitespace-nowrap max-w-[200px] overflow-hidden text-ellipsis"
                >
                  {skill}
                </span>
              ))}
              <span
                key="others"
                className="rounded-full bg-muted px-4 py-2 text-xs font-medium leading-snug text-foreground-secondary"
              >
                Other Skills...
              </span>
            </div>
          </div>
        </div>

        {/* footer */}
        <button
          onClick={handleSelectTrack}
          disabled={isPending}
          className="mt-6 flex cursor-pointer w-full items-center justify-center gap-2 rounded-md bg-primary/25 px-4 py-2 text-sm font-medium text-foreground transition-all duration-200 hover:bg-primary hover:text-muted hover:dark:text-foreground disabled:opacity-60 disabled:cursor-not-allowed"
        >
          {isPending ? (
            <>
              <Loader2 className="size-4 animate-spin" />
              Selecting...
            </>
          ) : (
            <>
              choose Track
              <ArrowUpRight className="size-4" />
            </>
          )}
        </button>
      </div>
    </article>
  );
}
