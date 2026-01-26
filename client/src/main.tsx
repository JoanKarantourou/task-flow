import { StrictMode } from "react";
import { createRoot } from "react-dom/client";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { Toaster } from "react-hot-toast";
import "./index.css";
import App from "./App.tsx";
import { AuthProvider } from "./context/AuthContext.tsx";
import { SignalRProvider } from "./context/SignalRContext.tsx";

// ============================================
// React Query Configuration
// ============================================

/**
 * Configure React Query client
 * Handles server state caching, synchronization, and background updates
 */
const queryClient = new QueryClient({
  defaultOptions: {
    queries: {
      refetchOnWindowFocus: false, // Don't refetch when window regains focus
      retry: 1, // Retry failed queries once
      staleTime: 5 * 60 * 1000, // Data stays fresh for 5 minutes
    },
  },
});

// ============================================
// App Root
// ============================================

createRoot(document.getElementById("root")!).render(
  <StrictMode>
    <QueryClientProvider client={queryClient}>
      <AuthProvider>
        <SignalRProvider>
          <Toaster
            position="top-right"
            toastOptions={{
              duration: 4000,
              style: {
                background: "#363636",
                color: "#fff",
              },
              success: {
                duration: 3000,
                iconTheme: {
                  primary: "#10b981",
                  secondary: "#fff",
                },
              },
              error: {
                duration: 5000,
                iconTheme: {
                  primary: "#ef4444",
                  secondary: "#fff",
                },
              },
            }}
          />
          <App />
        </SignalRProvider>
      </AuthProvider>
    </QueryClientProvider>
  </StrictMode>
);
