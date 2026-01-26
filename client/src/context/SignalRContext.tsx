// ============================================
// SignalR Context
// ============================================
// Provides SignalR connection state and methods to the entire app
// Manages connection lifecycle and real-time notifications

import {
  createContext,
  useContext,
  useState,
  useEffect,
  ReactNode,
} from "react";
import { HubConnection, HubConnectionState } from "@microsoft/signalr";
import { useQueryClient } from "@tanstack/react-query";
import toast from "react-hot-toast";
import signalRService from "../services/signalRService";
import type { Notification } from "../types";

// ============================================
// Context Type Definition
// ============================================

/**
 * SignalR Context Type
 * Defines what data and methods are available from the SignalR context
 */
interface SignalRContextType {
  // Current state
  connection: HubConnection | null; // SignalR connection object
  isConnected: boolean; // Is connection active?
  connectionState: HubConnectionState | null; // Current connection state

  // Connection methods
  joinProjectGroup: (projectId: string) => Promise<void>;
  leaveProjectGroup: (projectId: string) => Promise<void>;
}

// ============================================
// Create Context
// ============================================

/**
 * Create the SignalR Context
 * Provides undefined as default - will throw error if used outside provider
 */
const SignalRContext = createContext<SignalRContextType | undefined>(
  undefined
);

// ============================================
// SignalR Provider Component
// ============================================

/**
 * Props for SignalRProvider
 */
interface SignalRProviderProps {
  children: ReactNode;
}

/**
 * SignalR Provider Component
 * Wraps the app (or protected routes) to provide SignalR functionality
 * Automatically connects on mount and disconnects on unmount
 *
 * Usage:
 * <SignalRProvider>
 *   <App />
 * </SignalRProvider>
 */
export function SignalRProvider({ children }: SignalRProviderProps) {
  const [connection, setConnection] = useState<HubConnection | null>(null);
  const [isConnected, setIsConnected] = useState<boolean>(false);
  const [connectionState, setConnectionState] =
    useState<HubConnectionState | null>(null);

  const queryClient = useQueryClient();

  // ==========================================
  // Connection Lifecycle
  // ==========================================

  useEffect(() => {
    let isMounted = true;

    // Initialize SignalR connection
    async function initializeConnection() {
      try {
        console.log("🔌 Initializing SignalR connection from context...");
        await signalRService.startConnection();

        if (!isMounted) return;

        const conn = signalRService.getConnection();
        setConnection(conn);
        setIsConnected(signalRService.isConnected());
        setConnectionState(signalRService.getConnectionState());

        // Setup global event listeners
        setupEventListeners();

        console.log("✅ SignalR context initialized");
      } catch (error) {
        console.error("❌ Failed to initialize SignalR:", error);
        if (!isMounted) return;
        setIsConnected(false);
        setConnectionState(null);
      }
    }

    initializeConnection();

    // Cleanup on unmount
    return () => {
      isMounted = false;
      console.log("🔌 Cleaning up SignalR connection...");
      signalRService.stopConnection();
    };
  }, []);

  // ==========================================
  // Event Listeners Setup
  // ==========================================

  function setupEventListeners() {
    // Listen for task notifications
    signalRService.onTaskNotification((notification: Notification) => {
      console.log("📋 Task notification received in context:", notification);

      // Show toast notification
      toast.success(notification.message, {
        icon: "📋",
      });

      // Invalidate relevant queries to refetch data
      queryClient.invalidateQueries({ queryKey: ["tasks"] });

      // If notification has project data, invalidate project queries
      if (notification.data?.projectId) {
        queryClient.invalidateQueries({
          queryKey: ["projects", notification.data.projectId],
        });
        queryClient.invalidateQueries({
          queryKey: ["tasks", "project", notification.data.projectId],
        });
      }
    });

    // Listen for project notifications
    signalRService.onProjectUpdated((notification: Notification) => {
      console.log("📁 Project notification received in context:", notification);

      // Show toast notification
      toast.success(notification.message, {
        icon: "📁",
      });

      // Invalidate project queries
      queryClient.invalidateQueries({ queryKey: ["projects"] });

      if (notification.data?.projectId) {
        queryClient.invalidateQueries({
          queryKey: ["projects", notification.data.projectId],
        });
      }
    });

    // Listen for system notifications
    signalRService.onSystemNotification((notification: Notification) => {
      console.log("🔔 System notification received in context:", notification);

      // Show toast notification
      toast(notification.message, {
        icon: "🔔",
        duration: 5000,
      });
    });
  }

  // ==========================================
  // Group Management Methods
  // ==========================================

  async function joinProjectGroup(projectId: string): Promise<void> {
    try {
      await signalRService.joinProjectGroup(projectId);
    } catch (error) {
      console.error("❌ Failed to join project group:", error);
      toast.error("Failed to join project notifications");
    }
  }

  async function leaveProjectGroup(projectId: string): Promise<void> {
    try {
      await signalRService.leaveProjectGroup(projectId);
    } catch (error) {
      console.error("❌ Failed to leave project group:", error);
    }
  }

  // ==========================================
  // Context Value
  // ==========================================

  const value: SignalRContextType = {
    connection,
    isConnected,
    connectionState,
    joinProjectGroup,
    leaveProjectGroup,
  };

  return (
    <SignalRContext.Provider value={value}>
      {children}
    </SignalRContext.Provider>
  );
}

// ============================================
// useSignalR Hook
// ============================================

/**
 * Custom hook to access SignalR context
 * Throws error if used outside of SignalRProvider
 *
 * Usage:
 * const { connection, isConnected, joinProjectGroup } = useSignalR();
 */
export function useSignalR(): SignalRContextType {
  const context = useContext(SignalRContext);

  if (context === undefined) {
    throw new Error("useSignalR must be used within a SignalRProvider");
  }

  return context;
}

// Export the context for advanced use cases
export default SignalRContext;
