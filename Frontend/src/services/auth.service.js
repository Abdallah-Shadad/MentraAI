import axiosInstance from "./axiosInstance";

export const login = async (credentials) => {
  try {
    const response = await axiosInstance.post("/auth/login", credentials);
    return response.data;
  } catch (error) {
    console.error("Error logging in:", error);
    throw error;
  }
};

// Register
export const register = async (userData) => {
  try {
    const response = await axiosInstance.post("/auth/register", userData);
    return response.data;
  } catch (error) {
    console.error("Error registering:", error);
    throw error;
  }
};

// Logout
export const logout = async (queryClient) => {
  try {
    await axiosInstance.post("/auth/logout");
  } catch (error) {
    console.error("Error logging out from backend:", error);
  } finally {
    // Client-side session wiping
    if (typeof window !== "undefined") {
      try {
        sessionStorage.clear();
        localStorage.removeItem("user"); // clear any stored user summary
      } catch (storageError) {
        console.error("Error clearing client storage:", storageError);
      }

      // Clear React Query cache
      if (queryClient) {
        try {
          queryClient.clear();
        } catch (queryError) {
          console.error("Error clearing query client cache:", queryError);
        }
      }

      // Determine active locale for redirect
      const segments = window.location.pathname.split("/");
      const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "en";

      // Hard redirect to clear all in-memory React states
      window.location.href = `/${locale}/register/Login`;
    }
  }
};

// Get current user
export const getUser = async () => {
  try {
    const response = await axiosInstance.get("/users/me");
    return response.data;
  } catch (error) {
    console.error("Error getting user:", error);
    throw error;
  }
};

// Refresh token
export const refreshToken = async () => {
  try {
    const response = await axiosInstance.post("/auth/refresh");
    return response.data;
  } catch (error) {
    console.error("Error refreshing token:", error);
    throw error;
  }
};
