// ============================================
// Base API Client Configuration
// ============================================
// This file sets up Axios with interceptors for JWT authentication
// All API calls go through this configured instance

import axios, {
  AxiosError,
  AxiosInstance,
  InternalAxiosRequestConfig,
} from "axios";
import type { ApiError } from "../types";

// ============================================
// API Configuration
// ============================================

/**
 * Base URL for the API
 * In development: localhost:5000
 * In production: This would be your deployed API URL
 */
const API_BASE_URL = "http://localhost:5000/api";

/**
 * Create an Axios instance with default configuration
 * This is like creating a configured HttpClient in C#
 */
const apiClient: AxiosInstance = axios.create({
  baseURL: API_BASE_URL,
  headers: {
    "Content-Type": "application/json",
  },
  timeout: 30000, // 30 seconds timeout
});

// ============================================
// Request Interceptor
// ============================================
// This runs BEFORE every request is sent
// Similar to middleware in ASP.NET Core

/**
 * Add JWT token to every request automatically
 * This is like adding [Authorize] attribute in C# controllers
 */
apiClient.interceptors.request.use(
  (config: InternalAxiosRequestConfig) => {
    // Get the JWT token from localStorage
    const token = localStorage.getItem("accessToken");

    // If token exists, add it to the Authorization header
    if (token) {
      config.headers.Authorization = `Bearer ${token}`;
    }

    // Log the request for debugging (remove in production)
    console.log(`📤 ${config.method?.toUpperCase()} ${config.url}`);

    return config;
  },
  (error) => {
    console.error("❌ Request error:", error);
    return Promise.reject(error);
  }
);

// ============================================
// Response Interceptor
// ============================================
// This runs AFTER every response is received
// Perfect for handling errors globally

/**
 * Handle responses and errors globally
 * Similar to exception handling middleware in ASP.NET
 */
apiClient.interceptors.response.use(
  (response) => {
    // Log successful responses for debugging
    console.log(
      `✅ ${response.config.method?.toUpperCase()} ${response.config.url} - ${
        response.status
      }`
    );
    return response;
  },
  async (error: AxiosError<ApiError>) => {
    const originalRequest = error.config;

    // Log the error for debugging
    console.error(`❌ API Error:`, error.response?.data || error.message);

    // Handle 401 Unauthorized - Token expired
    if (error.response?.status === 401 && originalRequest) {
      // Try to refresh the token
      try {
        const refreshToken = localStorage.getItem("refreshToken");

        if (!refreshToken) {
          // No refresh token available, redirect to login
          redirectToLogin();
          return Promise.reject(error);
        }

        // Call refresh token endpoint
        const response = await axios.post(
          `${API_BASE_URL}/auth/refresh-token`,
          {
            token: localStorage.getItem("accessToken"),
            refreshToken: refreshToken,
          }
        );

        // Save new tokens
        const { token, refreshToken: newRefreshToken } = response.data;
        localStorage.setItem("accessToken", token);
        localStorage.setItem("refreshToken", newRefreshToken);

        // Retry the original request with new token
        if (originalRequest.headers) {
          originalRequest.headers.Authorization = `Bearer ${token}`;
        }
        return apiClient(originalRequest);
      } catch (refreshError) {
        // Refresh token failed, redirect to login
        console.error("Token refresh failed:", refreshError);
        redirectToLogin();
        return Promise.reject(refreshError);
      }
    }

    // Handle 403 Forbidden - User doesn't have permission
    if (error.response?.status === 403) {
      console.error("Access forbidden - insufficient permissions");
      // You could show a notification here
    }

    // Handle 404 Not Found
    if (error.response?.status === 404) {
      console.error("Resource not found");
    }

    // Handle 500 Internal Server Error
    if (error.response?.status === 500) {
      console.error("Server error - something went wrong on the backend");
    }

    return Promise.reject(error);
  }
);

// ============================================
// Helper Functions
// ============================================

/**
 * Redirect user to login page and clear authentication data
 */
function redirectToLogin() {
  localStorage.removeItem("accessToken");
  localStorage.removeItem("refreshToken");
  localStorage.removeItem("user");

  // Redirect to login (we'll set up routing later)
  window.location.href = "/login";
}

/**
 * Extract error message from API error response
 * Useful for displaying user-friendly error messages
 */
export function getErrorMessage(error: unknown): string {
  if (axios.isAxiosError(error)) {
    const apiError = error.response?.data as ApiError | undefined;

    // If API returned a structured error message
    if (apiError?.message) {
      return apiError.message;
    }

    // If API returned validation errors
    if (apiError?.errors) {
      const firstError = Object.values(apiError.errors)[0];
      return firstError?.[0] || "Validation error occurred";
    }

    // Generic error based on status code
    if (error.response?.status === 401) {
      return "Authentication failed. Please login again.";
    }
    if (error.response?.status === 403) {
      return "You don't have permission to perform this action.";
    }
    if (error.response?.status === 404) {
      return "The requested resource was not found.";
    }
    if (error.response?.status === 500) {
      return "A server error occurred. Please try again later.";
    }
  }

  return "An unexpected error occurred. Please try again.";
}

// ============================================
// Export
// ============================================

export default apiClient;
