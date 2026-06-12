import axiosInstance from "./axiosInstance";

// POST /api/v1/quizzes/generate — generate a quiz for a stage
export const generateQuiz = async (stageProgressId) => {
  const response = await axiosInstance.post("/quizzes/generate", {
    stageProgressId,
  });
  return response.data;
};

// GET /api/v1/quizzes/{quizId} — fetch an existing quiz
export const getQuiz = async (quizId) => {
  const response = await axiosInstance.get(`/quizzes/${quizId}`);
  return response.data;
};

// POST /api/v1/quizzes/{quizId}/submit — submit answers
// answers: [{ questionId: string, answer: string (label "A","B","C","D") }]
export const submitQuiz = async (quizId, answers) => {
  const response = await axiosInstance.post(`/quizzes/${quizId}/submit`, {
    answers,
  });
  return response.data;
};

// GET /api/v1/quizzes/history?stageProgressId={id}
export const getQuizHistory = async (stageProgressId) => {
  const response = await axiosInstance.get("/quizzes/history", {
    params: { stageProgressId },
  });
  return response.data;
};
