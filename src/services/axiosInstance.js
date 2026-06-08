import axios from "axios";

const API_URL = "https://mentraai-api.runasp.net/api/v1";

const axiosInstance = axios.create({
  baseURL: API_URL,
  withCredentials: true,
});

// const refreshAxios = axios.create({
//   baseURL: API_URL,
//   withCredentials: true,
// });

// axiosInstance.interceptors.response.use(
//   (response) => response,
//   async (error) => {
//     const originalRequest = error.config;

//     if (!error.response) {
//       return Promise.reject(error);
//     }

//     if (originalRequest.url?.includes("/auth/refresh")) {
//       window.location.href = "/register/login";
//       return Promise.reject(error);
//     }

//     if (error.response?.status === 401 && !originalRequest._retry) {
//       originalRequest._retry = true;

//       try {
//         await refreshAxios.post("/auth/refresh");

//         return axiosInstance(originalRequest);
//       } catch (err) {
//         window.location.href = "/register/login";
//       }
//     }

//     return Promise.reject(error);
//   },
// );

export default axiosInstance;
