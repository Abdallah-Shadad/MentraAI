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
      // Check if user actually has an active session before attempting a refresh.
      // If not, they are a guest; do not call /auth/refresh and propagate the 401.
      const hasSession = typeof window !== "undefined" && localStorage.getItem("has_session") === "true";
      if (!hasSession) {
        return Promise.reject(error);
      }

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

      // If the request that failed is the refresh endpoint itself, we must redirect to login if on a protected path
      if (originalRequest.url?.includes("/auth/refresh")) {
        if (typeof window !== "undefined") {
          localStorage.removeItem("has_session");
          localStorage.removeItem("user");
          const pathname = window.location.pathname;
          const segments = pathname.split("/");
          const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "";
          const localePrefix = locale ? `/${locale}` : "";
          const relativePath = locale ? pathname.substring(localePrefix.length) : pathname;
          const isProtectedPath = relativePath === "/student" || relativePath.startsWith("/student/");
          if (isProtectedPath) {
            window.location.href = `/${locale || "en"}/register/Login`;
          }
        }
        return Promise.reject(error);
      }

      originalRequest._retry = true;
      isRefreshing = true;

      try {
        // Send refresh token request (cookies are attached automatically)
        await axiosInstance.post("/auth/refresh", {});
        
        isRefreshing = false;
        processQueue(null);
        
        // Retry original request
        return axiosInstance(originalRequest);
      } catch (refreshError) {
        isRefreshing = false;
        processQueue(refreshError);
        
        // Session expired, redirect to login if on protected path
        if (typeof window !== "undefined") {
          localStorage.removeItem("has_session");
          localStorage.removeItem("user");
          const pathname = window.location.pathname;
          const segments = pathname.split("/");
          const locale = ["en", "ar"].includes(segments[1]) ? segments[1] : "";
          const localePrefix = locale ? `/${locale}` : "";
          const relativePath = locale ? pathname.substring(localePrefix.length) : pathname;
          const isProtectedPath = relativePath === "/student" || relativePath.startsWith("/student/");
          if (isProtectedPath) {
            window.location.href = `/${locale || "en"}/register/Login`;
          }
        }
        return Promise.reject(refreshError);
      }
    }

    return Promise.reject(error);
  }
);

export default axiosInstance;

