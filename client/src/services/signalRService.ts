// ============================================
// SignalR Service
// ============================================
// Manages real-time communication with the backend using SignalR
// Handles connection lifecycle, group subscriptions, and event listeners

import {
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";
import authService from "./authService";
import { NotificationType, type Notification } from "../types";

// ============================================
// SignalR Service
// ============================================

/**
 * SignalR Service
 * Provides real-time communication with the backend
 * Singleton pattern ensures single connection across the app
 */
class SignalRService {
  private connection: HubConnection | null = null;
  private readonly hubUrl = `${import.meta.env.VITE_API_URL || ""}/hubs/notifications`;

  /**
   * Initialize and start the SignalR connection
   * Automatically includes JWT token for authentication
   * @returns Promise that resolves when connection is established
   */
  async startConnection(): Promise<void> {
    try {
      // Don't create a new connection if one already exists and is connected
      if (
        this.connection &&
        this.connection.state === HubConnectionState.Connected
      ) {
        console.log("🔌 SignalR already connected");
        return;
      }

      console.log("🔌 Starting SignalR connection...");

      // Get JWT token for authentication
      const token = authService.getAccessToken();
      if (!token) {
        console.error("❌ No access token found for SignalR connection");
        throw new Error("Authentication required for SignalR");
      }

      // Build the connection with authentication
      this.connection = new HubConnectionBuilder()
        .withUrl(this.hubUrl, {
          accessTokenFactory: () => token,
        })
        .withAutomaticReconnect() // Auto-reconnect on disconnect
        .configureLogging(LogLevel.Information)
        .build();

      // Setup connection event handlers
      this.connection.onreconnecting((error) => {
        console.log("🔄 SignalR reconnecting...", error);
      });

      this.connection.onreconnected((connectionId) => {
        console.log("✅ SignalR reconnected:", connectionId);
      });

      this.connection.onclose((error) => {
        console.log("🔌 SignalR connection closed", error);
      });

      // Start the connection
      await this.connection.start();
      console.log("✅ SignalR connection established");
    } catch (error) {
      console.error("❌ Failed to start SignalR connection:", error);
      throw error;
    }
  }

  /**
   * Stop the SignalR connection
   * Should be called when user logs out or app unmounts
   * @returns Promise that resolves when connection is stopped
   */
  async stopConnection(): Promise<void> {
    try {
      if (this.connection) {
        console.log("🔌 Stopping SignalR connection...");
        await this.connection.stop();
        this.connection = null;
        console.log("✅ SignalR connection stopped");
      }
    } catch (error) {
      console.error("❌ Failed to stop SignalR connection:", error);
      throw error;
    }
  }

  /**
   * Join a project-specific notification group
   * Subscribes to real-time updates for tasks/activities in the project
   * @param projectId - The project's unique identifier
   * @returns Promise that resolves when joined
   */
  async joinProjectGroup(projectId: string): Promise<void> {
    try {
      if (!this.connection) {
        console.error("❌ SignalR connection not established");
        return;
      }

      console.log(`📢 Joining project group: ${projectId}`);
      await this.connection.invoke("JoinProjectGroup", projectId);
      console.log(`✅ Joined project group: ${projectId}`);
    } catch (error) {
      console.error("❌ Failed to join project group:", error);
      throw error;
    }
  }

  /**
   * Leave a project-specific notification group
   * Unsubscribes from real-time updates for the project
   * @param projectId - The project's unique identifier
   * @returns Promise that resolves when left
   */
  async leaveProjectGroup(projectId: string): Promise<void> {
    try {
      if (!this.connection) {
        console.error("❌ SignalR connection not established");
        return;
      }

      console.log(`📢 Leaving project group: ${projectId}`);
      await this.connection.invoke("LeaveProjectGroup", projectId);
      console.log(`✅ Left project group: ${projectId}`);
    } catch (error) {
      console.error("❌ Failed to leave project group:", error);
      throw error;
    }
  }

  /**
   * Send a test notification (for debugging)
   * @returns Promise that resolves when test notification is sent
   */
  async sendTestNotification(): Promise<void> {
    try {
      if (!this.connection) {
        console.error("❌ SignalR connection not established");
        return;
      }

      console.log("📨 Sending test notification...");
      await this.connection.invoke("SendTestNotification");
      console.log("✅ Test notification sent");
    } catch (error) {
      console.error("❌ Failed to send test notification:", error);
      throw error;
    }
  }

  /**
   * Register a callback for task creation notifications
   * @param callback - Function to call when a task is created
   */
  onTaskCreated(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveTaskNotification", (notification: Notification) => {
      if (notification.type === NotificationType.TaskCreated) {
        console.log("📋 Task created notification received:", notification);
        callback(notification);
      }
    });
  }

  /**
   * Register a callback for task update notifications
   * @param callback - Function to call when a task is updated
   */
  onTaskUpdated(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveTaskNotification", (notification: Notification) => {
      if (notification.type === NotificationType.TaskUpdated) {
        console.log("📋 Task updated notification received:", notification);
        callback(notification);
      }
    });
  }

  /**
   * Register a callback for task assignment notifications
   * @param callback - Function to call when a task is assigned
   */
  onTaskAssigned(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveTaskNotification", (notification: Notification) => {
      if (notification.type === NotificationType.TaskAssigned) {
        console.log("📋 Task assigned notification received:", notification);
        callback(notification);
      }
    });
  }

  /**
   * Register a callback for any task notification
   * @param callback - Function to call when any task notification is received
   */
  onTaskNotification(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveTaskNotification", (notification: Notification) => {
      console.log("📋 Task notification received:", notification);
      callback(notification);
    });
  }

  /**
   * Register a callback for project update notifications
   * @param callback - Function to call when a project is updated
   */
  onProjectUpdated(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveProjectNotification", (notification: Notification) => {
      console.log("📁 Project notification received:", notification);
      callback(notification);
    });
  }

  /**
   * Register a callback for system-wide notifications
   * @param callback - Function to call when a system notification is received
   */
  onSystemNotification(callback: (notification: Notification) => void): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.on("ReceiveSystemNotification", (notification: Notification) => {
      console.log("🔔 System notification received:", notification);
      callback(notification);
    });
  }

  /**
   * Remove a specific event listener
   * @param eventName - Name of the event to stop listening to
   */
  off(eventName: string): void {
    if (!this.connection) {
      console.error("❌ SignalR connection not established");
      return;
    }

    this.connection.off(eventName);
    console.log(`🔇 Stopped listening to: ${eventName}`);
  }

  /**
   * Get the current connection state
   * @returns Current HubConnectionState or null if no connection
   */
  getConnectionState(): HubConnectionState | null {
    return this.connection?.state ?? null;
  }

  /**
   * Get the underlying SignalR connection object
   * Use with caution - prefer using the service methods
   * @returns HubConnection or null if not connected
   */
  getConnection(): HubConnection | null {
    return this.connection;
  }

  /**
   * Check if the connection is currently active
   * @returns true if connected, false otherwise
   */
  isConnected(): boolean {
    return this.connection?.state === HubConnectionState.Connected;
  }
}

// ============================================
// Export Singleton Instance
// ============================================
// Create a single instance to use throughout the app

const signalRService = new SignalRService();
export default signalRService;
