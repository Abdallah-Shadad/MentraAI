import axiosInstance from "./axiosInstance";

export const getStageResources1 = async (stageId) => {
  console.log("Getting stage resources 1 for stage:", stageId);
  try {
    const response = await axiosInstance.get(`/stages/${stageId}/enter`);
    return response.data;
  } catch (error) {
    console.error("Error fetching stage resources 1:", error);
    throw error;
  }
};

export const getStageResources2 = async (stageId) => {
  console.log("Getting stage resources 2 for stage:", stageId);
  try {
    const response = await axiosInstance.get(`/stages/${stageId}/resources`);
    return response.data;
  } catch (error) {
    console.error("Error fetching stage resources 2:", error);
    throw error;
  }
};
