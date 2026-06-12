import { useMutation, useQuery, useQueryClient } from "@tanstack/react-query";
import {
  generateRoadmap,
  getCurrentRoadmap,
} from "../services/roadmap.service";

export const useGenerateRoadmap = (options = {}) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: generateRoadmap,
    ...options,
    onSuccess: (...args) => {
      queryClient.invalidateQueries({ queryKey: ["current-roadmap"] });
      queryClient.invalidateQueries({ queryKey: ["career-track"] });
      if (options.onSuccess) {
        options.onSuccess(...args);
      }
    },
    onError: (error, ...args) => {
      console.log(error);
      if (options.onError) {
        options.onError(error, ...args);
      }
    },
  });
};

export const useGetCurrentRoadmap = () => {
  return useQuery({
    queryKey: ["current-roadmap"],
    queryFn: getCurrentRoadmap,

    onError: (error) => {
      console.log(error);
    },
  });
};
