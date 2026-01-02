// ============================================
// TypeScript Type Definitions for TaskFlow
// ============================================
// These interfaces define the shape of data we receive from the API
// They match the C# entities and DTOs from the backend

// ============================================
// ENUMS
// ============================================
// These match the enums in TaskFlow.Domain/Enums/

/**
 * Task status enum - represents the current state of a task
 * Matches C#: TaskFlow.Domain.Enums.TaskStatus
 */
export enum TaskStatus {
  Todo = 0,
  InProgress = 1,
  InReview = 2,
  Done = 3,
  Cancelled = 4,
}

/**
 * Task priority enum - represents the urgency level
 * Matches C#: TaskFlow.Domain.Enums.TaskPriority
 */
export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2,
  Critical = 3,
}

/**
 * Project status enum - represents the current state of a project
 * Matches C#: TaskFlow.Domain.Enums.ProjectStatus
 */
export enum ProjectStatus {
  Active = 0,
  OnHold = 1,
  Completed = 2,
  Archived = 3,
}

// ============================================
// DOMAIN ENTITIES
// ============================================
// These match the domain entities in TaskFlow.Domain/Entities/

/**
 * User entity - represents an authenticated user
 * Matches C#: TaskFlow.Domain.Entities.User
 */
export interface User {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  createdAt: string; // ISO 8601 date string from API
  updatedAt: string;
}

/**
 * Project entity - represents a project that contains tasks
 * Matches C#: TaskFlow.Domain.Entities.Project
 */
export interface Project {
  id: string;
  name: string;
  description?: string; // Optional field (nullable in C#)
  status: ProjectStatus;
  ownerId: string;
  startDate?: string; // ISO 8601 date string, optional
  dueDate?: string; // ISO 8601 date string, optional
  createdAt: string;
  updatedAt: string;
  // Navigation properties (populated by API)
  owner?: User;
  tasks?: Task[];
  memberCount?: number; // Calculated field from API
}

/**
 * Task entity - represents a task within a project
 * Matches C#: TaskFlow.Domain.Entities.TaskItem
 * Note: Called "Task" here instead of "TaskItem" for simplicity
 */
export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  projectId: string;
  assigneeId?: string; // Optional - task might be unassigned
  dueDate?: string; // ISO 8601 date string
  createdAt: string;
  updatedAt: string;
  // Navigation properties (populated by API)
  project?: Project;
  assignee?: User;
  comments?: Comment[];
  commentCount?: number; // Calculated field from API
}

/**
 * Comment entity - represents a comment on a task
 * Matches C#: TaskFlow.Domain.Entities.Comment
 */
export interface Comment {
  id: string;
  content: string;
  taskId: string;
  authorId: string;
  createdAt: string;
  updatedAt: string;
  // Navigation properties
  author?: User;
}

/**
 * Project Member - represents a user's membership in a project
 * Matches C#: TaskFlow.Domain.Entities.ProjectMember
 */
export interface ProjectMember {
  projectId: string;
  userId: string;
  role: string; // "Owner", "Admin", "Member"
  // Navigation properties
  project?: Project;
  user?: User;
}

// ============================================
// AUTHENTICATION DTOs
// ============================================
// These match the DTOs in TaskFlow.Application/DTOs/

/**
 * Login request DTO
 * Sent to: POST /api/auth/login
 */
export interface LoginRequest {
  email: string;
  password: string;
}

/**
 * Register request DTO
 * Sent to: POST /api/auth/register
 */
export interface RegisterRequest {
  email: string;
  password: string;
  firstName: string;
  lastName: string;
}

/**
 * Authentication response DTO
 * Received from: POST /api/auth/login and /api/auth/register
 */
export interface AuthResponse {
  token: string; // JWT access token
  refreshToken: string; // Refresh token for getting new access tokens
  user: User; // User information
  expiresAt: string; // When the access token expires (ISO 8601)
}

/**
 * Refresh token request DTO
 * Sent to: POST /api/auth/refresh-token
 */
export interface RefreshTokenRequest {
  token: string; // Current access token
  refreshToken: string; // Current refresh token
}

// ============================================
// TASK DTOs
// ============================================

/**
 * Create task request DTO
 * Sent to: POST /api/tasks
 */
export interface CreateTaskRequest {
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  projectId: string;
  assigneeId?: string;
  dueDate?: string; // ISO 8601 date string
}

/**
 * Update task request DTO
 * Sent to: PUT /api/tasks/{id}
 */
export interface UpdateTaskRequest {
  title?: string;
  description?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  assigneeId?: string;
  dueDate?: string;
}

// ============================================
// PROJECT DTOs
// ============================================

/**
 * Create project request DTO
 * Sent to: POST /api/projects
 */
export interface CreateProjectRequest {
  name: string;
  description?: string;
  status: ProjectStatus;
  startDate?: string;
  dueDate?: string;
}

/**
 * Update project request DTO
 * Sent to: PUT /api/projects/{id}
 */
export interface UpdateProjectRequest {
  name?: string;
  description?: string;
  status?: ProjectStatus;
  startDate?: string;
  dueDate?: string;
}

// ============================================
// API RESPONSE WRAPPERS
// ============================================

/**
 * Generic API response wrapper for lists
 * Used when API returns paginated or filtered lists
 */
export interface ApiListResponse<T> {
  items: T[];
  totalCount: number;
  pageNumber?: number;
  pageSize?: number;
}

/**
 * API error response
 * Received when API returns an error
 */
export interface ApiError {
  message: string;
  errors?: Record<string, string[]>; // Validation errors by field
  statusCode: number;
}

// ============================================
// SIGNALR NOTIFICATION TYPES
// ============================================

/**
 * Real-time notification received via SignalR
 */
export interface Notification {
  id: string;
  type: NotificationType;
  title: string;
  message: string;
  timestamp: string;
  isRead: boolean;
  data?: any; // Additional data specific to notification type
}

/**
 * Types of notifications the system can send
 */
export enum NotificationType {
  TaskCreated = "TaskCreated",
  TaskUpdated = "TaskUpdated",
  TaskAssigned = "TaskAssigned",
  TaskCommented = "TaskCommented",
  ProjectUpdated = "ProjectUpdated",
  MemberAdded = "MemberAdded",
}

// ============================================
// UTILITY TYPES
// ============================================

/**
 * Form state for handling loading/error states
 * Useful for form components
 */
export interface FormState {
  isLoading: boolean;
  error: string | null;
  success: boolean;
}

/**
 * Pagination parameters for API requests
 */
export interface PaginationParams {
  pageNumber: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
}

/**
 * Filter parameters for tasks
 */
export interface TaskFilterParams extends PaginationParams {
  projectId?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  assigneeId?: string;
  search?: string;
}
