import axiosInstance from "./axiosInstance";

export const submitOnboarding = async (answers) => {
  try {
    const response = await axiosInstance.post("onboarding/answers", answers);
    return response.data;
  } catch (error) {
    console.error("Error submitting onboarding:", error);
    throw error;
  }
};
