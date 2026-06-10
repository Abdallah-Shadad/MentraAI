import { useQuery } from "@tanstack/react-query";
import { getUserProfile } from "../services/user.service";

export const useGetUserProfile = () => {
  return useQuery({
    queryKey: ["user-profile"],
    queryFn: getUserProfile,
  });
};
