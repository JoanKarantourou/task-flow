// ============================================
// Status Colors Utilities
// ============================================
// Helper functions to get consistent colors for badges and status indicators
// Returns Tailwind CSS classes for background and text colors

import { TaskStatus, TaskPriority, ProjectStatus } from "../types";

/**
 * Get Tailwind CSS classes for task status badges
 * @param status - TaskStatus enum value
 * @returns Tailwind class string (bg-* text-*)
 */
export function getTaskStatusColor(status: TaskStatus): string {
  switch (status) {
    case TaskStatus.Todo:
      return "bg-blue-100 text-blue-800 border-blue-200";
    case TaskStatus.InProgress:
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case TaskStatus.InReview:
      return "bg-purple-100 text-purple-800 border-purple-200";
    case TaskStatus.Done:
      return "bg-green-100 text-green-800 border-green-200";
    case TaskStatus.Cancelled:
      return "bg-gray-100 text-gray-800 border-gray-200";
    default:
      return "bg-gray-100 text-gray-800 border-gray-200";
  }
}

/**
 * Get Tailwind CSS classes for task priority badges
 * @param priority - TaskPriority enum value
 * @returns Tailwind class string (bg-* text-*)
 */
export function getTaskPriorityColor(priority: TaskPriority): string {
  switch (priority) {
    case TaskPriority.Low:
      return "bg-gray-100 text-gray-700 border-gray-200";
    case TaskPriority.Medium:
      return "bg-blue-100 text-blue-700 border-blue-200";
    case TaskPriority.High:
      return "bg-orange-100 text-orange-700 border-orange-200";
    case TaskPriority.Critical:
      return "bg-red-100 text-red-700 border-red-200";
    default:
      return "bg-gray-100 text-gray-700 border-gray-200";
  }
}

/**
 * Get Tailwind CSS classes for project status badges
 * @param status - ProjectStatus enum value
 * @returns Tailwind class string (bg-* text-*)
 */
export function getProjectStatusColor(status: ProjectStatus): string {
  switch (status) {
    case ProjectStatus.Active:
      return "bg-green-100 text-green-800 border-green-200";
    case ProjectStatus.OnHold:
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case ProjectStatus.Completed:
      return "bg-blue-100 text-blue-800 border-blue-200";
    case ProjectStatus.Archived:
      return "bg-gray-100 text-gray-800 border-gray-200";
    default:
      return "bg-gray-100 text-gray-800 border-gray-200";
  }
}

/**
 * Get user-friendly label for task status
 * @param status - TaskStatus enum value
 * @returns Human-readable status label
 */
export function getTaskStatusLabel(status: TaskStatus): string {
  switch (status) {
    case TaskStatus.Todo:
      return "To Do";
    case TaskStatus.InProgress:
      return "In Progress";
    case TaskStatus.InReview:
      return "In Review";
    case TaskStatus.Done:
      return "Done";
    case TaskStatus.Cancelled:
      return "Cancelled";
    default:
      return "Unknown";
  }
}

/**
 * Get user-friendly label for task priority
 * @param priority - TaskPriority enum value
 * @returns Human-readable priority label
 */
export function getTaskPriorityLabel(priority: TaskPriority): string {
  switch (priority) {
    case TaskPriority.Low:
      return "Low";
    case TaskPriority.Medium:
      return "Medium";
    case TaskPriority.High:
      return "High";
    case TaskPriority.Critical:
      return "Critical";
    default:
      return "Unknown";
  }
}

/**
 * Get user-friendly label for project status
 * @param status - ProjectStatus enum value
 * @returns Human-readable status label
 */
export function getProjectStatusLabel(status: ProjectStatus): string {
  switch (status) {
    case ProjectStatus.Active:
      return "Active";
    case ProjectStatus.OnHold:
      return "On Hold";
    case ProjectStatus.Completed:
      return "Completed";
    case ProjectStatus.Archived:
      return "Archived";
    default:
      return "Unknown";
  }
}

/**
 * Get header color for Kanban columns
 * @param status - TaskStatus enum value
 * @returns Tailwind class string for column header
 */
export function getKanbanColumnColor(status: TaskStatus): string {
  switch (status) {
    case TaskStatus.Todo:
      return "bg-blue-500 text-white";
    case TaskStatus.InProgress:
      return "bg-yellow-500 text-white";
    case TaskStatus.InReview:
      return "bg-purple-500 text-white";
    case TaskStatus.Done:
      return "bg-green-500 text-white";
    default:
      return "bg-gray-500 text-white";
  }
}
