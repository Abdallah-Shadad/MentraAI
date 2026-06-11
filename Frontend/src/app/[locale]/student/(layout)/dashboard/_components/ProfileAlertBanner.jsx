"use client";
import { AlertTriangle, ArrowRight } from "lucide-react";
import { useRouter } from "@/lib/i18n/navigation";

export default function ProfileAlertBanner() {
  const router = useRouter();

  return (
    <div className="w-full mb-8 bg-amber-500/10 border border-amber-500/30 rounded-lg p-5 flex flex-col md:flex-row items-center justify-between gap-4 shadow-[0_0_15px_rgba(245,158,11,0.05)] backdrop-blur-xs animate-pulse">
      <div className="flex items-center gap-3">
        <div className="p-2 rounded-full bg-amber-500/20 text-amber-500">
          <AlertTriangle className="w-6 h-6" />
        </div>
        <div>
          <h4 className="text-lg font-bold text-amber-500">
            Profile Incomplete
          </h4>
          <p className="text-sm text-foreground/80">
            You haven't selected a learning career track yet. Choose your track to unlock your personalized roadmap.
          </p>
        </div>
      </div>

      <button
        onClick={() => router.push("/student")}
        className="flex items-center gap-2 px-5 py-2.5 bg-amber-500 hover:bg-amber-600 text-black font-bold rounded-md transition-all active:scale-[0.98] cursor-pointer shrink-0 shadow-lg shadow-amber-500/20"
      >
        <span>Choose Career Track</span>
        <ArrowRight className="w-4 h-4" />
      </button>
    </div>
  );
}
