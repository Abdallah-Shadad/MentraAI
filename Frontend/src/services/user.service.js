import axiosInstance from "./axiosInstance";

export const getUserProfile = async () => {
  try {
    const response = await axiosInstance.get("/users/me");
    if (typeof window !== "undefined") {
      localStorage.setItem("has_session", "true");
    }
    return response.data;
  } catch (error) {
    throw error;
  }
};

export const updateUserProfile = async (profileData) => {
  try {
    const response = await axiosInstance.put("/users/me", profileData);
    return response.data;
  } catch (error) {
    throw error;
  }
};
