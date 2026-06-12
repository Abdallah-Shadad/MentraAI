import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import {
  generateQuiz,
  getQuiz,
  submitQuiz,
  getQuizHistory,
} from "../services/quiz.service";

export const useGenerateQuiz = (options = {}) => {
  return useMutation({
    mutationFn: generateQuiz,
    ...options,
  });
};

export const useGetQuiz = (quizId, options = {}) => {
  return useQuery({
    queryKey: ["quiz", quizId],
    queryFn: () => getQuiz(quizId),
    enabled: !!quizId,
    ...options,
  });
};

export const useSubmitQuiz = (options = {}) => {
  const queryClient = useQueryClient();
  return useMutation({
    mutationFn: ({ quizId, answers }) => submitQuiz(quizId, answers),
    onSuccess: (data, variables) => {
      // Invalidate related queries so they refetch the updated progress / roadmap / resources
      queryClient.invalidateQueries({ queryKey: ["current-roadmap"] });
      queryClient.invalidateQueries({ queryKey: ["roadmap"] });
      queryClient.invalidateQueries({ queryKey: ["quiz-history"] });
      queryClient.invalidateQueries({ queryKey: ["stage-resources"] });
      if (options.onSuccess) {
        options.onSuccess(data, variables);
      }
    },
    ...options,
  });
};

export const useQuizHistory = (stageProgressId, options = {}) => {
  return useQuery({
    queryKey: ["quiz-history", stageProgressId],
    queryFn: () => getQuizHistory(stageProgressId),
    enabled: !!stageProgressId,
    ...options,
  });
};
