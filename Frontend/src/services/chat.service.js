import axiosInstance from "./axiosInstance";

/**
 * Create a new chat conversation.
 * @param {string|null} title
 * @returns {Promise<object>} The conversation response payload
 */
export const createConversation = async (title = null) => {
  const response = await axiosInstance.post("/chat/conversations", { title });
  return response.data;
};

/**
 * Fetch all chat conversations for the logged-in student.
 * @returns {Promise<object>} The conversation list payload
 */
export const getConversations = async () => {
  const response = await axiosInstance.get("/chat/conversations");
  return response.data;
};

/**
 * Delete a chat conversation and clear backend memory.
 * @param {string} conversationId
 * @returns {Promise<void>}
 */
export const deleteConversation = async (conversationId) => {
  await axiosInstance.delete(`/chat/conversations/${conversationId}`);
};

/**
 * Query the backend proxy check health for the AI service.
 * @returns {Promise<boolean>} True if online and active, false otherwise.
 */
export const checkChatHealth = async () => {
  try {
    const response = await axiosInstance.get("/chat/health");
    return response.data?.success !== false;
  } catch (error) {
    console.error("Chat health check failed:", error);
    return false;
  }
};

/**
 * Stream responses directly from the .NET Chat gateway message endpoint.
 * Handles 401 Token Expiration automatically by triggering /auth/refresh and retrying once.
 * 
 * @param {string} conversationId 
 * @param {object} chatRequest { query, careerTrack, stage, lessonId, quizDetails, quizScore }
 * @param {object} options { onToken: (text) => void, onError: (err) => void, signal: AbortSignal }
 * @param {number} retryCount Internal usage to prevent infinite retry loops
 */
export const streamMessage = async (conversationId, chatRequest, { onToken, onError, signal }) => {
  // Normalize base URL to prevent duplicate /api/v1 path segments
  let baseUrl = process.env.NEXT_PUBLIC_API_URL || "";
  if (baseUrl.endsWith("/api/v1")) {
    baseUrl = baseUrl.slice(0, -7);
  } else if (baseUrl.endsWith("/api/v1/")) {
    baseUrl = baseUrl.slice(0, -8);
  } else if (baseUrl.endsWith("/")) {
    baseUrl = baseUrl.slice(0, -1);
  }
  const url = `${baseUrl}/api/v1/chat/conversations/${conversationId}/messages`;

  try {
    const response = await fetch(url, {
      method: "POST",
      headers: {
        "Content-Type": "application/json",
      },
      body: JSON.stringify(chatRequest),
      credentials: "include", // Inherits cookies/JWT from session context
      signal,
    });

    if (!response.ok) {
      let errorMessage = `HTTP error! status: ${response.status}`;
      try {
        const errJson = await response.json();
        if (errJson?.error?.message) {
          errorMessage = errJson.error.message;
        }
      } catch (_) {
        // Fallback to default HTTP status text
      }
      throw new Error(errorMessage);
    }

    // Process stream response
    const reader = response.body.getReader();
    const decoder = new TextDecoder();
    
    // Register immediate abort listener to kill reader and release connection resources
    const abortHandler = () => {
      reader.cancel().catch(() => {});
    };

    if (signal) {
      if (signal.aborted) {
        await reader.cancel();
        return;
      }
      signal.addEventListener("abort", abortHandler);
    }

    try {
      while (true) {
        const { done, value } = await reader.read();
        if (done) break;

        const chunk = decoder.decode(value, { stream: true });
        onToken(chunk);
      }
    } finally {
      if (signal) {
        signal.removeEventListener("abort", abortHandler);
      }
    }
  } catch (error) {
    if (error.name === "AbortError") {
      console.log("Stream reader request aborted successfully.");
      return;
    }
    onError(error);
  }
};
