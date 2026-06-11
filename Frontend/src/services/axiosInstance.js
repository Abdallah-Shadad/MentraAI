import axios from "axios";

const axiosInstance = axios.create({
  baseURL: process.env.NEXT_PUBLIC_API_URL,
  withCredentials: true,
});

let isRefreshing = false;
let failedQueue = [];

const processQueue = (error, token = null) => {
  failedQueue.forEach((prom) => {
    if (error) {
      prom.reject(error);
    } else {
      prom.resolve(token);
    }
  });
  failedQueue = [];
};

axiosInstance.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;

    if (!error.response) {
      return Promise.reject(error);
    }

    // Intercept 401 errors
    if (error.response.status === 401 && !originalRequest._retry) {
      // If we are already refreshing, queue the request until the refresh is complete
      if (isRefreshing) {
        return new Promise((resolve, reject) => {
          failedQueue.push({ resolve, reject });
        })
          .then(() => {
            return axiosInstance(originalRequest);
          })
          .catch((err) => {
            return Promise.reject(err);
          });
      }

      // If the request that failed is the refresh endpoint itself, we must redirect to login
      if (originalRequest.url?.includes("/auth/refresh")) {
        if (typeof window !== "undefined") {
          window.location.href = "/en/register/Login";
        }
        return Promise.reject(error);
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Send refresh token request (cookies are attached automatically)
        await axiosInstance.post("/auth/refresh");
        
        isRefreshing = false;
        processQueue(null);
        
        // Retry original request
        return axiosInstance(originalRequest);
      } catch (refreshError) {
        isRefreshing = false;
        processQueue(refreshError);
        
        // Session expired, redirect to login
        if (typeof window !== "undefined") {
          window.location.href = "/en/register/Login";
        }
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosInstance;

