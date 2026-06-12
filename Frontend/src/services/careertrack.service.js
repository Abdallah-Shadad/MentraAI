import axiosInstance from "./axiosInstance";

export async function getMyTrack() {
  try {
    const response = await axiosInstance.get("/career-tracks/my-track");
    return response.data;
  } catch (error) {
    const status = error?.response?.status;
    if (status !== 404 && status !== 422) {
      console.error("Error getting my track:", error);
    }
    throw error;
  }
}

export async function postTrackSelection(trackId) {
  try {
    const response = await axiosInstance.post("/career-tracks/select", {
      careerTrackId: trackId,
    });
    return response.data;
  } catch (error) {
    console.error("Error selecting track:", error);
    throw error;
  }
}

export async function getTracksRecommended() {
  try {
    const response = await axiosInstance.post("/career-tracks/recommendation");
    return response.data;
  } catch (error) {
    console.error("Error getting recommended tracks:", error);
    throw error;
  }
}
