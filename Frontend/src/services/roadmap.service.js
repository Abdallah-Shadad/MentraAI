import axiosInstance from "./axiosInstance";

export const generateRoadmap = async () => {
  const response = await axiosInstance.post("/roadmaps/generate");
  return response.data;
};

export const getCurrentRoadmap = async () => {
  const response = await axiosInstance.get("/roadmaps/current");
  return response.data;
};
