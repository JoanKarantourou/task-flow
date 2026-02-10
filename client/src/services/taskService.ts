// ============================================
// Task Service
// ============================================
// Handles all task-related API operations
// CRUD operations for tasks: Create, Read, Update, Delete

import apiClient, { getErrorMessage } from "./api";
import {
  type Task,
  type CreateTaskRequest,
  type UpdateTaskRequest,
  TaskStatus,
  type TaskFilterParams,
  type ApiListResponse,
  type PagedResult,
} from "../types";

// ============================================
// Task Service
// ============================================

/**
 * Task Service
 * Provides methods for managing tasks
 * Similar to a TaskService in C# that would use repositories
 */
class TaskService {
  /**
   * Get a single task by ID
   * @param taskId - The task's unique identifier (GUID)
   * @returns Promise with task details
   */
  async getTaskById(taskId: string): Promise<Task> {
    try {
      console.log(`📋 Fetching task: ${taskId}`);

      // GET /api/tasks/{id}
      const response = await apiClient.get<Task>(`/tasks/${taskId}`);

      console.log(`✅ Fetched task: ${response.data.title}`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch task:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Get all tasks for a specific project
   * @param projectId - The project's unique identifier
   * @returns Promise with array of tasks
   */
  async getTasksByProjectId(projectId: string): Promise<Task[]> {
    try {
      console.log(`📋 Fetching tasks for project: ${projectId}`);

      // GET /api/tasks/project/{projectId}
      const response = await apiClient.get<Task[]>(
        `/tasks/project/${projectId}`
      );

      console.log(`✅ Found ${response.data.length} tasks`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch project tasks:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Get all tasks across all projects (with optional filtering and pagination)
   * @param filters - Optional filter and pagination parameters
   * @returns Promise with paginated tasks
   */
  async getAllTasks(filters?: TaskFilterParams): Promise<PagedResult<Task>> {
    try {
      console.log("📋 Fetching all tasks");

      // Build query string from filters
      const queryParams = new URLSearchParams();
      if (filters?.projectId)
        queryParams.append("projectId", filters.projectId);
      if (filters?.status !== undefined)
        queryParams.append("status", filters.status.toString());
      if (filters?.priority !== undefined)
        queryParams.append("priority", filters.priority.toString());
      if (filters?.assigneeId)
        queryParams.append("assigneeId", filters.assigneeId);
      if (filters?.search) queryParams.append("searchTerm", filters.search);
      if (filters?.pageNumber)
        queryParams.append("pageNumber", filters.pageNumber.toString());
      if (filters?.pageSize)
        queryParams.append("pageSize", filters.pageSize.toString());
      if (filters?.sortBy) queryParams.append("sortBy", filters.sortBy);
      if (filters?.sortOrder)
        queryParams.append("sortDescending", (filters.sortOrder === "desc").toString());

      // GET /api/tasks?searchTerm=...&status=...
      const queryString = queryParams.toString();
      const response = await apiClient.get<PagedResult<Task>>(
        `/tasks${queryString ? `?${queryString}` : ""}`
      );

      console.log(`✅ Found ${response.data.items.length} of ${response.data.totalCount} tasks`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to fetch tasks:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Create a new task
   * @param taskData - Task creation data (title, description, etc.)
   * @returns Promise with the created task
   */
  async createTask(taskData: CreateTaskRequest): Promise<Task> {
    try {
      console.log(`📝 Creating new task: ${taskData.title}`);

      // POST /api/tasks
      const response = await apiClient.post<Task>("/tasks", taskData);

      console.log(`✅ Task created successfully: ${response.data.id}`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to create task:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Update an existing task
   * @param taskId - The task's unique identifier
   * @param updateData - Fields to update (partial update supported)
   * @returns Promise with the updated task
   */
  async updateTask(
    taskId: string,
    updateData: UpdateTaskRequest
  ): Promise<Task> {
    try {
      console.log(`✏️ Updating task: ${taskId}`);

      // PUT /api/tasks/{id}
      const response = await apiClient.put<Task>(
        `/tasks/${taskId}`,
        updateData
      );

      console.log(`✅ Task updated successfully`);
      return response.data;
    } catch (error) {
      console.error("❌ Failed to update task:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Update only the status of a task (useful for drag-and-drop Kanban)
   * @param taskId - The task's unique identifier
   * @param newStatus - The new status value
   * @returns Promise with the updated task
   */
  async updateTaskStatus(
    taskId: string,
    newStatus: TaskStatus
  ): Promise<Task> {
    try {
      console.log(`🔄 Updating task status to: ${TaskStatus[newStatus]}`);

      // PUT /api/tasks/{id} with only status field
      const response = await this.updateTask(taskId, { status: newStatus });

      console.log(`✅ Task status updated successfully`);
      return response;
    } catch (error) {
      console.error(
        "❌ Failed to update task status:",
        getErrorMessage(error)
      );
      throw error;
    }
  }

  /**
   * Assign a task to a user
   * @param taskId - The task's unique identifier
   * @param assigneeId - The user's unique identifier
   * @returns Promise with the updated task
   */
  async assignTask(taskId: string, assigneeId: string | null): Promise<Task> {
    try {
      console.log(
        `👤 ${assigneeId ? "Assigning" : "Unassigning"} task: ${taskId}`
      );

      // PUT /api/tasks/{id} with only assigneeId field
      const response = await this.updateTask(taskId, { assigneeId: assigneeId || undefined });

      console.log(`✅ Task ${assigneeId ? "assigned" : "unassigned"} successfully`);
      return response;
    } catch (error) {
      console.error("❌ Failed to assign task:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Delete a task
   * Note: This endpoint returns 501 (Not Implemented) until backend is complete
   * @param taskId - The task's unique identifier
   * @returns Promise that resolves when deletion is complete
   */
  async deleteTask(taskId: string): Promise<void> {
    try {
      console.log(`🗑️ Deleting task: ${taskId}`);

      // DELETE /api/tasks/{id}
      await apiClient.delete(`/tasks/${taskId}`);

      console.log(`✅ Task deleted successfully`);
    } catch (error) {
      console.error("❌ Failed to delete task:", getErrorMessage(error));
      throw error;
    }
  }

  /**
   * Get tasks with pagination
   * Useful for large lists with search/filter functionality
   * @param params - Pagination and filter parameters
   * @returns Promise with paginated task list
   */
  async getTasksPaginated(
    params?: TaskFilterParams
  ): Promise<ApiListResponse<Task>> {
    try {
      console.log("📋 Fetching tasks with pagination");

      // Build query string from parameters
      const queryParams = new URLSearchParams();
      if (params?.pageNumber)
        queryParams.append("pageNumber", params.pageNumber.toString());
      if (params?.pageSize)
        queryParams.append("pageSize", params.pageSize.toString());
      if (params?.projectId) queryParams.append("projectId", params.projectId);
      if (params?.status !== undefined)
        queryParams.append("status", params.status.toString());
      if (params?.priority !== undefined)
        queryParams.append("priority", params.priority.toString());
      if (params?.assigneeId)
        queryParams.append("assigneeId", params.assigneeId);
      if (params?.search) queryParams.append("search", params.search);

      // GET /api/tasks?pageNumber=1&pageSize=10&search=...
      const response = await apiClient.get<ApiListResponse<Task>>(
        `/tasks?${queryParams.toString()}`
      );

      console.log(
        `✅ Fetched ${response.data.items.length} of ${response.data.totalCount} tasks`
      );
      return response.data;
    } catch (error) {
      console.error(
        "❌ Failed to fetch paginated tasks:",
        getErrorMessage(error)
      );
      throw error;
    }
  }
}

// ============================================
// Export Singleton Instance
// ============================================
// Create a single instance to use throughout the app

const taskService = new TaskService();
export default taskService;
