import { useMutation, useQuery } from "@tanstack/react-query";
import {
  generateRoadmap,
  getCurrentRoadmap,
} from "../services/roadmap.service";

export const useGenerateRoadmap = () => {
  return useMutation({
    mutationFn: generateRoadmap,

    onError: (error) => {
      console.log(error);
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
