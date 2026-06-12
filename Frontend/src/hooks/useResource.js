import { useQuery } from "@tanstack/react-query";
import {
  getStageResources1,
  getStageResources2,
} from "../services/resource.service";

export const useStageResources = (stageProgressId) => {
  return useQuery({
    queryKey: ["stage-resources", stageProgressId],
    queryFn: async () => {
      try {
        // Try getting existing resources first (fast path)
        console.log("Attempting fast-path GET resources for:", stageProgressId);
        const data = await getStageResources2(stageProgressId);
        return data;
      } catch (error) {
        const status = error?.response?.status;
        const code = error?.response?.data?.error?.code;

        // If status is 422 or code is RESOURCES_NOT_FETCHED, resources are missing.
        // Trigger the POST enter / generation logic (slow path)
        if (status === 422 || code === "RESOURCES_NOT_FETCHED") {
          console.log("Resources not found. Triggering slow-path POST enter stage:", stageProgressId);
          const data = await getStageResources1(stageProgressId);
          return data;
        }
        
        throw error;
      }
    },
    enabled: !!stageProgressId,
    retry: false,
    refetchOnWindowFocus: false,
  });
};
