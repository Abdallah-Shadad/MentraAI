import { useQuery, useMutation } from "@tanstack/react-query";
import {
  getMyTrack,
  postTrackSelection,
  getTracksRecommended,
} from "../services/careertrack.service";

export function useCareerTrack() {
  return useQuery({
    queryKey: ["career-track"],
    queryFn: async () => {
      try {
        return await getMyTrack();
      } catch (error) {
        const status = error?.response?.status;
        if (status === 404 || status === 422) {
          return { data: null };
        }
        throw error;
      }
    },
    retry: false,
  });
}

export function useTrackSelection() {
  return useMutation({
    mutationFn: async (trackId) => {
      const response = await postTrackSelection(trackId);
      return response;
    },

    onError: (err) => {
      console.error(err.response?.data?.message || "An error occurred");
    },
  });
}

export function useTracksRecommended() {
  return useQuery({
    queryKey: ["track-recommended"],
    queryFn: getTracksRecommended,
  });
}
