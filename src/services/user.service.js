import axiosInstance from "./axiosInstance";

export const getUserProfile = async () => {
  try {
    const response = await axiosInstance.get("/users/me");
    return response.data;
  } catch (error) {
    throw error;
  }
};
