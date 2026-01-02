// ============================================
// Authentication Context
// ============================================
// Provides authentication state and methods to the entire app
// Similar to a scoped service in .NET that manages user session

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";
import authService from "../services/authService";
import type {
  User,
  LoginRequest,
  RegisterRequest,
  AuthResponse,
} from "../types";

// ============================================
// Context Type Definition
// ============================================

/**
 * Authentication Context Type
 * Defines what data and methods are available from the auth context
 */
interface AuthContextType {
  // Current state
  user: User | null; // Currently logged-in user
  isAuthenticated: boolean; // Is user logged in?
  isLoading: boolean; // Is auth state being determined?

  // Authentication methods
  login: (credentials: LoginRequest) => Promise<void>;
  register: (data: RegisterRequest) => Promise<void>;
  logout: () => void;

  // Utility methods
  updateUser: (user: User) => void; // Update user info after profile change
}

// ============================================
// Create Context
// ============================================

/**
 * Create the Auth Context
 * Initially null - will be provided by AuthProvider
 */
const AuthContext = createContext<AuthContextType | null>(null);

// ============================================
// Auth Provider Component
// ============================================

/**
 * Props for AuthProvider component
 */
interface AuthProviderProps {
  children: ReactNode; // Child components that will have access to auth context
}

/**
 * Authentication Provider Component
 * Wraps the entire app to provide authentication state
 *
 * Usage:
 * <AuthProvider>
 *   <App />
 * </AuthProvider>
 */
export function AuthProvider({ children }: AuthProviderProps) {
  // ==========================================
  // State Management
  // ==========================================

  /**
   * Current authenticated user
   * null if not logged in
   */
  const [user, setUser] = useState<User | null>(null);

  /**
   * Loading state - true while determining if user is authenticated
   * Prevents flash of login screen on page load
   */
  const [isLoading, setIsLoading] = useState<boolean>(true);

  // ==========================================
  // Initialization - Check if User is Already Logged In
  // ==========================================

  /**
   * On component mount, check if user is already authenticated
   * This happens when user refreshes the page or comes back later
   * Similar to checking if JWT token exists and is valid in C#
   */
  useEffect(() => {
    const initializeAuth = () => {
      console.log("🔐 Initializing authentication state");

      try {
        // Check if user has valid tokens in localStorage
        const isAuth = authService.isAuthenticated();

        if (isAuth) {
          // User has tokens - get user data from localStorage
          const currentUser = authService.getCurrentUser();

          if (currentUser) {
            setUser(currentUser);
            console.log("✅ User authenticated:", currentUser.email);
          } else {
            console.log("⚠️ Tokens exist but no user data found");
          }
        } else {
          console.log("ℹ️ No authentication found");
        }
      } catch (error) {
        console.error("❌ Error initializing auth:", error);
        // Clear any corrupted auth data
        authService.logout();
      } finally {
        // Done checking - stop loading
        setIsLoading(false);
      }
    };

    initializeAuth();
  }, []); // Empty dependency array = run once on mount

  // ==========================================
  // Authentication Methods
  // ==========================================

  /**
   * Login user with email and password
   * @param credentials - Email and password
   */
  const login = async (credentials: LoginRequest): Promise<void> => {
    try {
      console.log("🔐 Logging in user:", credentials.email);

      // Call auth service to login
      const response: AuthResponse = await authService.login(credentials);

      // Update state with logged-in user
      setUser(response.user);

      console.log("✅ Login successful!");
    } catch (error) {
      console.error("❌ Login failed:", error);
      // Re-throw error so component can handle it (show error message)
      throw error;
    }
  };

  /**
   * Register a new user account
   * @param data - Registration data (email, password, firstName, lastName)
   */
  const register = async (data: RegisterRequest): Promise<void> => {
    try {
      console.log("📝 Registering new user:", data.email);

      // Call auth service to register
      const response: AuthResponse = await authService.register(data);

      // Update state with newly registered user
      setUser(response.user);

      console.log("✅ Registration successful!");
    } catch (error) {
      console.error("❌ Registration failed:", error);
      // Re-throw error so component can handle it
      throw error;
    }
  };

  /**
   * Logout the current user
   * Clears all authentication state and redirects to login
   */
  const logout = (): void => {
    console.log("👋 Logging out user");

    // Call auth service to clear tokens
    authService.logout();

    // Clear user state
    setUser(null);

    console.log("✅ Logout complete");

    // Note: Navigation to login page will be handled by the component
    // or by a protected route component
  };

  /**
   * Update the current user's information
   * Called after user updates their profile
   * @param updatedUser - Updated user object
   */
  const updateUser = (updatedUser: User): void => {
    console.log("💾 Updating user data");

    // Update in localStorage
    authService.updateCurrentUser(updatedUser);

    // Update in state
    setUser(updatedUser);

    console.log("✅ User data updated");
  };

  // ==========================================
  // Context Value
  // ==========================================

  /**
   * The value that will be provided to all consuming components
   * This is what components get when they use useAuth()
   */
  const value: AuthContextType = {
    user,
    isAuthenticated: !!user, // Convert to boolean: user exists = true, null = false
    isLoading,
    login,
    register,
    logout,
    updateUser,
  };

  // ==========================================
  // Render Provider
  // ==========================================

  /**
   * Provide the auth context to all children
   * While loading, you could show a loading spinner instead of children
   */
  return (
    <AuthContext.Provider value={value}>
      {/* Show loading indicator while determining auth state */}
      {isLoading ? (
        <div className="min-h-screen flex items-center justify-center bg-gray-100">
          <div className="text-center">
            <div className="inline-block animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
            <p className="mt-4 text-gray-600">Loading...</p>
          </div>
        </div>
      ) : (
        children
      )}
    </AuthContext.Provider>
  );
}

// ============================================
// Custom Hook - useAuth
// ============================================

/**
 * Custom hook to use the auth context
 * This makes it easy to access auth state in any component
 *
 * Usage in a component:
 * const { user, login, logout, isAuthenticated } = useAuth();
 *
 * Similar to dependency injection in C#:
 * public MyController(IAuthService authService) { }
 */
export function useAuth(): AuthContextType {
  const context = useContext(AuthContext);

  // Ensure hook is used within AuthProvider
  if (!context) {
    throw new Error(
      "useAuth must be used within an AuthProvider. " +
        "Make sure your component is wrapped in <AuthProvider>."
    );
  }

  return context;
}

// ============================================
// Default Export
// ============================================

export default AuthContext;
