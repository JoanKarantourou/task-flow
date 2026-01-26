// ============================================
// Protected Route Component
// ============================================
// Wrapper for routes that require authentication
// Redirects to login page if user is not authenticated

import { Navigate, Outlet } from "react-router-dom";
import { useAuth } from "../../context/AuthContext";
import { SignalRProvider } from "../../context/SignalRContext";
import MainLayout from "../layout/MainLayout";

/**
 * Protected Route Component
 * Checks authentication status and wraps authenticated routes
 * with MainLayout and SignalR connection
 *
 * Usage in App.tsx:
 * <Route element={<ProtectedRoute />}>
 *   <Route path="/dashboard" element={<DashboardPage />} />
 *   <Route path="/projects" element={<ProjectsPage />} />
 * </Route>
 */
export default function ProtectedRoute() {
  const { isAuthenticated, isLoading } = useAuth();

  // Show loading state while checking authentication
  if (isLoading) {
    return (
      <div className="flex h-screen items-center justify-center bg-gray-50">
        <div className="text-center">
          <div className="inline-block h-8 w-8 animate-spin rounded-full border-4 border-solid border-primary-600 border-r-transparent"></div>
          <p className="mt-4 text-gray-600">Loading...</p>
        </div>
      </div>
    );
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  // Render protected content wrapped with MainLayout and SignalR
  return (
    <SignalRProvider>
      <MainLayout>
        <Outlet />
      </MainLayout>
    </SignalRProvider>
  );
}
