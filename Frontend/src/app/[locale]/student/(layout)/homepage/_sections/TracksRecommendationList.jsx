"use client";

import TrackRecommendationCard from "../_components/TrackRecommendationCard";
import ErrorState from "@/components/reusable/ErrorState";
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
  DialogDescription,
} from "@/components/ui/dialog";

import { useTracksRecommended } from "@/hooks/useCareerTrack";

export default function TracksRecommendationList({ isOpen, onOpenChange }) {
  const { data, isLoading, isError, error } = useTracksRecommended();

  const tracks = data?.data?.recommendedTracks || [];

  return (
    <Dialog open={isOpen} onOpenChange={onOpenChange}>
      <DialogContent className="min-w-[calc(100%-10rem)] h-[95vh] overflow-y-auto border-border [&>button]:text-destructive [&>button]:hover:text-destructive-foreground [&>button]:cursor-pointer">
        <DialogHeader>
          <DialogTitle className="text-2xl font-bold text-center text-foreground">
            Select a Track
          </DialogTitle>
          <DialogDescription className="text-foreground-muted text-center">
            Choose a track to start your learning journey
          </DialogDescription>
        </DialogHeader>
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mt-6">
          {isError && (
            <ErrorState message={error?.response?.data?.error?.message} />
          )}
          {isLoading && <div>Loading...</div>}
          {tracks?.map((track) => (
            <TrackRecommendationCard
              key={track.careerTrackId}
              track={track}
            />
          ))}
        </div>
      </DialogContent>
    </Dialog>
  );
}
