import { useMutation, useQuery } from "@tanstack/react-query";
import { register, login, getUser } from "../services/auth.service";

export const useRegister = () => {
  return useMutation({
    mutationFn: async (userData) => {
      const response = await register(userData);
      return response;
    },

    onError: (err) => {
      console.error(err.response?.data?.message || "An error occurred");
    },
  });
};

export const useLogin = () => {
  return useMutation({
    mutationFn: async (userData) => {
      const response = await login(userData);
      return response;
    },

    onError: (err) => {
      console.error(err.response?.data?.message || "An error occurred");
    },
  });
};

export const useGetCurrentUser = () => {
  return useQuery({
    queryKey: ["user"],
    queryFn: async () => {
      const response = await getUser();
      return response;
    },

    onError: (err) => {
      console.error(err.response?.data?.message || "An error occurred");
    },
  });
};
