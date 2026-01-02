// ============================================
// Project Service
// ============================================
// Handles all project-related API operations
// CRUD operations for projects: Create, Read, Update, Delete

import apiClient, { getErrorMessage } from "./api";
import type {
  Project,
  CreateProjectRequest,
  UpdateProjectRequest,
  ApiListResponse,
} from "../types";

// ============================================
// Project Service
// ============================================

/**
 * Project Service
 * Provides methods for managing projects
 * Similar to a ProjectService in C# that would use repositories
 */
class ProjectService {
  /**
   * Get all projects for the current authenticated user
   * Returns projects where the user is the owner or a member
   * @returns Promise with array of projects
   */
  async getMyProjects(): Promise<Project[]> {
    try {
      console.log("📋 Fetching user's projects");

      // GET /api/projects
      // Your API should filter by current user based on JWT token
      const response = await apiClient.get<Project[]>("/projects");

      console.log(`✅ Found ${response.data.length} projects`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch projects:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Get a single project by ID
   * @param projectId - The project's unique identifier (GUID)
   * @returns Promise with project details
   */
  async getProjectById(projectId: string): Promise<Project> {
    try {
      console.log(`📋 Fetching project: ${projectId}`);

      // GET /api/projects/{id}
      const response = await apiClient.get<Project>(`/projects/${projectId}`);

      console.log(`✅ Fetched project: ${response.data.name}`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch project:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Create a new project
   * @param projectData - Project creation data (name, description, etc.)
   * @returns Promise with the created project
   */
  async createProject(projectData: CreateProjectRequest): Promise<Project> {
    try {
      console.log(`📝 Creating new project: ${projectData.name}`);

      // POST /api/projects
      const response = await apiClient.post<Project>("/projects", projectData);

      console.log(`✅ Project created successfully: ${response.data.id}`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to create project:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Update an existing project
   * @param projectId - The project's unique identifier
   * @param updateData - Fields to update (partial update supported)
   * @returns Promise with the updated project
   */
  async updateProject(
    projectId: string,
    updateData: UpdateProjectRequest
  ): Promise<Project> {
    try {
      console.log(`✏️ Updating project: ${projectId}`);

      // PUT /api/projects/{id}
      const response = await apiClient.put<Project>(
        `/projects/${projectId}`,
        updateData
      );

      console.log(`✅ Project updated successfully`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to update project:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Delete a project
   * @param projectId - The project's unique identifier
   * @returns Promise that resolves when deletion is complete
   */
  async deleteProject(projectId: string): Promise<void> {
    try {
      console.log(`🗑️ Deleting project: ${projectId}`);

      // DELETE /api/projects/{id}
      await apiClient.delete(`/projects/${projectId}`);

      console.log(`✅ Project deleted successfully`);
    } catch (error) {
      console.error("❌ Failed to delete project:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Get all projects where the user is the owner
   * Useful for dashboard statistics or project management views
   * @returns Promise with array of owned projects
   */
  async getOwnedProjects(): Promise<Project[]> {
    try {
      console.log("📋 Fetching owned projects");

      // GET /api/projects/owned
      // This endpoint should be implemented in your API to filter by owner
      // For now, we'll use the main endpoint and filter client-side
      const allProjects = await this.getMyProjects();

      // Filter to only show owned projects
      // In a real app, this filtering should happen on the backend for performance
      const ownedProjects = allProjects.filter((project) => {
        // You'd need to compare with current user's ID
        // For now, return all (backend should handle this)
        return true;
      });

      console.log(`✅ Found ${ownedProjects.length} owned projects`);
      return ownedProjects;
    } catch (error) {
      console.error(
        "❌ Failed to fetch owned projects:",
        getErrorMessage(error)
      );
      throw error;
    }
  }

  /**
   * Get projects with pagination and filtering
   * Useful for large lists with search/filter functionality
   * @param params - Pagination and filter parameters
   * @returns Promise with paginated project list
   */
  async getProjectsPaginated(params?: {
    pageNumber?: number;
    pageSize?: number;
    search?: string;
    status?: number;
  }): Promise<ApiListResponse<Project>> {
    try {
      console.log("📋 Fetching projects with pagination");

      // Build query string from parameters
      const queryParams = new URLSearchParams();
      if (params?.pageNumber)
        queryParams.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize)
        queryParams.append("pageSize", params.pageSize.toString());
      if (params?.search) queryParams.append("search", params.search);
      if (params?.status !== undefined)
        queryParams.append("status", params.status.toString());

      // GET /api/projects?pageNumber=1&pageSize=10&search=...
      const response = await apiClient.get<ApiListResponse<Project>>(
        `/projects?${queryParams.toString()}`
      );

      console.log(
        `✅ Fetched ${response.data.items.length} of ${response.data.totalCount} projects`
      );
      return response.data;
    } catch (error) {
      console.error(
        "❌ Failed to fetch paginated projects:",
        getErrorMessage(error)
      );
      throw error;
    }
  }

  /**
   * Archive a project (soft delete)
   * Sets project status to Archived instead of deleting
   * @param projectId - The project's unique identifier
   * @returns Promise with the updated project
   */
  async archiveProject(projectId: string): Promise<Project> {
    try {
      console.log(`📦 Archiving project: ${projectId}`);

      // Update project status to Archived (status = 3)
      const response = await this.updateProject(projectId, {
        status: 3, // ProjectStatus.Archived
      });

      console.log(`✅ Project archived successfully`);
      return response;
    } catch (error) {
      console.error("❌ Failed to archive project:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Restore an archived project
   * Sets project status back to Active
   * @param projectId - The project's unique identifier
   * @returns Promise with the updated project
   */
  async restoreProject(projectId: string): Promise<Project> {
    try {
      console.log(`♻️ Restoring project: ${projectId}`);

      // Update project status to Active (status = 0)
      const response = await this.updateProject(projectId, {
        status: 0, // ProjectStatus.Active
      });

      console.log(`✅ Project restored successfully`);
      return response;
    } catch (error) {
      console.error("❌ Failed to restore project:", getErrorMessage(error));
      throw error;
    }
  }
}

// ============================================
// Export Singleton Instance
// ============================================
// Create a single instance to use throughout the app

const projectService = new ProjectService();
export default projectService;
