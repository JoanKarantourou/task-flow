// ============================================
// Authentication Service
// ============================================
// Handles user authentication: login, register, logout, token management
// This is the service layer that components will use for auth operations

import apiClient, { getErrorMessage } from "./api";
import type {
  LoginRequest,
  RegisterRequest,
  AuthResponse,
  User,
} from "../types";

// ============================================
// Local Storage Keys
// ============================================
// Centralized keys to avoid typos and make refactoring easier

const STORAGE_KEYS = {
  ACCESS_TOKEN: "accessToken",
  REFRESH_TOKEN: "refreshToken",
  USER: "user",
} as const;

// ============================================
// Authentication Service
// ============================================

/**
 * Authentication Service
 * Provides methods for user authentication and session management
 * Similar to an AuthService in C# that would handle JWT operations
 */
class AuthService {
  /**
   * Register a new user account
   * @param data - Registration data (email, password, firstName, lastName)
   * @returns Promise with user data and tokens
   */
  async register(data: RegisterRequest): Promise<AuthResponse> {
    try {
      console.log("📝 Registering new user:", data.email);

      // Send POST request to /api/auth/register
      const response = await apiClient.post<AuthResponse>(
        "/auth/register",
        data
      );

      // Save authentication data to localStorage
      this.saveAuthData(response.data);

      console.log("✅ Registration successful!");
      return response.data;
    } catch (error) {
      console.error("❌ Registration failed:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Login with email and password
   * @param credentials - Login credentials (email, password)
   * @returns Promise with user data and tokens
   */
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    try {
      console.log("🔐 Logging in user:", credentials.email);

      // Send POST request to /api/auth/login
      const response = await apiClient.post<AuthResponse>(
        "/auth/login",
        credentials
      );

      // Save authentication data to localStorage
      this.saveAuthData(response.data);

      console.log("✅ Login successful!");
      return response.data;
    } catch (error) {
      console.error("❌ Login failed:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Logout the current user
   * Clears all authentication data from localStorage
   */
  logout(): void {
    console.log("👋 Logging out user");

    // Clear all auth data from localStorage
    localStorage.removeItem(STORAGE_KEYS.ACCESS_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.REFRESH_TOKEN);
    localStorage.removeItem(STORAGE_KEYS.USER);

    // Note: In a real app, you might also want to:
    // 1. Call an API endpoint to invalidate the refresh token on the server
    // 2. Clear any other user-specific data
    // 3. Disconnect from SignalR hub

    console.log("✅ Logout complete");
  }

  /**
   * Refresh the access token using the refresh token
   * Called automatically by the API interceptor when token expires
   * @returns Promise with new tokens
   */
  async refreshToken(): Promise<AuthResponse> {
    try {
      console.log("🔄 Refreshing access token");

      const accessToken = localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
      const refreshToken = localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);

      if (!accessToken || !refreshToken) {
        throw new Error("No tokens available for refresh");
      }

      // Send POST request to /api/auth/refresh-token
      const response = await apiClient.post<AuthResponse>(
        "/auth/refresh-token",
        {
          token: accessToken,
          refreshToken: refreshToken,
        }
      );

      // Save new authentication data
      this.saveAuthData(response.data);

      console.log("✅ Token refreshed successfully");
      return response.data;
    } catch (error) {
      console.error("❌ Token refresh failed:", getErrorMessage(error));
      // Clear auth data and force re-login
      this.logout();
      throw error;
    }
  }

  /**
   * Get the current authenticated user from localStorage
   * @returns User object or null if not authenticated
   */
  getCurrentUser(): User | null {
    try {
      const userJson = localStorage.getItem(STORAGE_KEYS.USER);
      if (!userJson) {
        return null;
      }

      // Parse the JSON string back to User object
      return JSON.parse(userJson) as User;
    } catch (error) {
      console.error("❌ Error parsing user data:", error);
      return null;
    }
  }

  /**
   * Get the current access token
   * @returns Access token string or null
   */
  getAccessToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.ACCESS_TOKEN);
  }

  /**
   * Get the current refresh token
   * @returns Refresh token string or null
   */
  getRefreshToken(): string | null {
    return localStorage.getItem(STORAGE_KEYS.REFRESH_TOKEN);
  }

  /**
   * Check if user is currently authenticated
   * @returns true if user has valid tokens, false otherwise
   */
  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    const user = this.getCurrentUser();

    // User is authenticated if both token and user data exist
    return !!(token && user);
  }

  /**
   * Save authentication data to localStorage
   * @param authData - Authentication response from API
   */
  private saveAuthData(authData: AuthResponse): void {
    // Save tokens
    localStorage.setItem(STORAGE_KEYS.ACCESS_TOKEN, authData.token);
    localStorage.setItem(STORAGE_KEYS.REFRESH_TOKEN, authData.refreshToken);

    // Save user data as JSON string
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(authData.user));

    console.log("💾 Authentication data saved to localStorage");
  }

  /**
   * Update the current user's information in localStorage
   * Useful after profile updates
   * @param user - Updated user object
   */
  updateCurrentUser(user: User): void {
    localStorage.setItem(STORAGE_KEYS.USER, JSON.stringify(user));
    console.log("💾 User data updated in localStorage");
  }
}

// ============================================
// Export Singleton Instance
// ============================================
// Create a single instance that will be used throughout the app
// This is similar to registering a service as Singleton in .NET DI

const authService = new AuthService();
export default authService;
