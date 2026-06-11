"use client";
import { useGetUserProfile } from "@/hooks/useUser";
import { useCareerTrack } from "@/hooks/useCareerTrack";


import ProfileAlertBanner from "./_components/ProfileAlertBanner";
import ProfileInfoCard from "./_components/ProfileInfoCard";
import CareerTrackCard from "./_components/CareerTrackCard";
import ProfileEditForm from "./_components/ProfileEditForm";

import { User2, Loader2 } from "lucide-react";
import ErrorState from "@/components/reusable/ErrorState";

export default function DashboardPage() {
  // Queries
  const {
    data: profileData,
    isLoading: isProfileLoading,
    isError: isProfileError,
    error: profileError,
  } = useGetUserProfile();

  const {
    data: trackResponse,
    isLoading: isTrackLoading,
    isError: isTrackError,
  } = useCareerTrack();



  const isLoading = isProfileLoading || isTrackLoading;

  if (isLoading) {
    return (
      <div className="min-h-screen bg-background flex flex-col items-center justify-center gap-4">
        <Loader2 className="w-10 h-10 text-primary animate-spin" />
        <span className="text-sm font-semibold text-foreground-muted">Loading your profile data...</span>
      </div>
    );
  }

  if (isProfileError) {
    const errorMsg = profileError?.response?.data?.error?.message || "Failed to load user profile.";
    return (
      <div className="min-h-screen bg-background p-8 flex items-center justify-center">
        <div className="w-full max-w-lg">
          <ErrorState message={errorMsg} close={() => window.location.reload()} />
        </div>
      </div>
    );
  }

  const profile = profileData?.data;
  const track = trackResponse?.data;

  // Rule: Profile is incomplete if no career track is chosen
  const hasNoTrack = isTrackError || !track;

  return (
    <div className="min-h-screen bg-background relative py-8">
      <section className="main-container space-y-8 px-4 sm:px-6 lg:px-8">
        
        {/* Header Section */}
        <div className="flex flex-col sm:flex-row items-start sm:items-center justify-between gap-4 border-b border-border/40 pb-6 w-full">
          <div>
            <h2 className="font-heading font-semibold text-3xl text-foreground mb-2 flex items-center gap-2">
              <User2 className="w-8 h-8 text-primary" />
              <span>Full Profile Dashboard</span>
            </h2>
            <p className="text-foreground-muted text-sm">
              Manage your personal metadata, track status, and learning preferences.
            </p>
          </div>
        </div>

        {/* Dynamic Alert Banner */}
        {hasNoTrack && <ProfileAlertBanner />}

        {/* Identity Card */}
        <ProfileInfoCard profile={profile} />

        {/* Career Track Status Card */}
        <CareerTrackCard trackData={track} isError={isTrackError} />

        {/* Form Card */}
        <div className="space-y-4">
          <div className="px-1">
            <h4 className="text-xl font-bold text-foreground">Edit Metadata Profile</h4>
            <p className="text-sm text-foreground-muted">
              Update educational level, work sector, organization size, and skills inventory.
            </p>
          </div>
          <ProfileEditForm profile={profile} />
        </div>

      </section>
    </div>
  );
}
