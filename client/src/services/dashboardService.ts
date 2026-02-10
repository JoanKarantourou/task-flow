// ============================================
// Dashboard Service
// ============================================
// Handles all dashboard-related API operations

import apiClient, { getErrorMessage } from "./api";
import type { DashboardStats } from "../types";

// ============================================
// Dashboard Service
// ============================================

/**
 * Dashboard Service
 * Provides methods for fetching dashboard data
 */
class DashboardService {
  /**
   * Get dashboard statistics
   * @returns Promise with dashboard stats including project/task counts
   */
  async getStats(): Promise<DashboardStats> {
    try {
      console.log("📊 Fetching dashboard stats");

      // GET /api/dashboard/stats
      const response = await apiClient.get<DashboardStats>("/dashboard/stats");

      console.log("✅ Dashboard stats fetched successfully");
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch dashboard stats:", getErrorMessage(error));
      throw error;
    }
  }
}

// ============================================
// Export Singleton Instance
// ============================================

const dashboardService = new DashboardService();
export default dashboardService;
