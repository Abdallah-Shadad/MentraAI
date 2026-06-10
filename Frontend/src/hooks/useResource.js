import { useQuery } from "@tanstack/react-query";
import {
  getStageResources1,
  getStageResources2,
} from "../services/resource.service";

export const useGetStageResources1 = (stageId) => {
  return useQuery({
    queryKey: ["stage-resources", stageId],
    queryFn: () => getStageResources1(stageId),
    enabled: !!stageId,
    onError: (error) => {
      console.log(error);
    },
  });
};

export const useGetStageResources2 = (stageId) => {
  return useQuery({
    queryKey: ["stage-resources", stageId],
    queryFn: () => getStageResources2(stageId),
    enabled: !!stageId,
    onError: (error) => {
      console.log(error);
    },
  });
};
