"use client";
import { User, Mail, Calendar, CheckCircle, Clock } from "lucide-react";

export default function ProfileInfoCard({ profile }) {
  if (!profile) return null;

  const fullName = `${profile.firstName || ""} ${profile.lastName || ""}`.trim() || "Student User";
  const formattedDate = profile.createdAt
    ? new Date(profile.createdAt).toLocaleDateString(undefined, {
        year: "numeric",
        month: "long",
        day: "numeric",
      })
    : "Recently";

  return (
    <div className="bg-card/50 backdrop-blur-sm border border-border rounded-lg p-6 shadow-shadow-card relative overflow-hidden">
      {/* Subtle brand glow inside card */}
      <div className="absolute -top-12 -right-12 w-24 h-24 bg-primary/10 blur-xl rounded-full pointer-events-none"></div>

      <div className="flex flex-col md:flex-row md:items-center justify-between gap-6">
        <div className="flex items-center gap-4">
          <div className="w-16 h-16 rounded-full bg-linear-to-br from-primary to-secondary flex items-center justify-center shadow-shadow-neon shrink-0">
            <span className="text-foreground font-extrabold text-2xl">
              {profile.firstName ? profile.firstName[0].toUpperCase() : "S"}
            </span>
          </div>
          <div>
            <h3 className="text-2xl font-bold text-foreground mb-1">
              {fullName}
            </h3>
            <span className="inline-flex items-center gap-1 text-xs font-semibold px-2.5 py-0.5 rounded-full bg-primary/20 text-primary border border-primary/30">
              User Profile ID: {profile.userId?.substring(0, 8) || "N/A"}
            </span>
          </div>
        </div>

        <div className="grid grid-cols-1 sm:grid-cols-2 lg:flex items-center gap-4 text-sm text-foreground-muted">
          <div className="flex items-center gap-2 bg-surface-elevated/40 px-3 py-2 rounded-md border border-border/50">
            <Mail className="w-4 h-4 text-primary" />
            <div>
              <p className="text-[10px] text-foreground-muted uppercase tracking-wider font-bold">Email Address</p>
              <p className="text-foreground text-xs font-medium">{profile.email}</p>
            </div>
          </div>

          <div className="flex items-center gap-2 bg-surface-elevated/40 px-3 py-2 rounded-md border border-border/50">
            <Calendar className="w-4 h-4 text-secondary" />
            <div>
              <p className="text-[10px] text-foreground-muted uppercase tracking-wider font-bold">Joined MentraAI</p>
              <p className="text-foreground text-xs font-medium">{formattedDate}</p>
            </div>
          </div>

          <div className="flex items-center gap-2 bg-surface-elevated/40 px-3 py-2 rounded-md border border-border/50">
            {profile.isOnboarded ? (
              <>
                <CheckCircle className="w-4 h-4 text-emerald-500" />
                <div>
                  <p className="text-[10px] text-foreground-muted uppercase tracking-wider font-bold">Status</p>
                  <p className="text-emerald-400 text-xs font-bold">Onboarded</p>
                </div>
              </>
            ) : (
              <>
                <Clock className="w-4 h-4 text-amber-500" />
                <div>
                  <p className="text-[10px] text-foreground-muted uppercase tracking-wider font-bold">Status</p>
                  <p className="text-amber-400 text-xs font-bold">Pending</p>
                </div>
              </>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}
