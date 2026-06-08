import { useMutation } from "@tanstack/react-query";
import { submitOnboarding } from "@/services/onboarding.service";

export const useOnboarding = () => {
  return useMutation({
    mutationFn: async (answers) => {
      const response = await submitOnboarding(answers);
      return response;
    },

    onError: (err) => {
      console.error(err.response?.data?.message || "An error occurred");
    },
  });
};
