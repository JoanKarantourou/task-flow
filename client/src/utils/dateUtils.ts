// ============================================
// Date Utilities
// ============================================
// Helper functions for formatting and working with dates
// Uses date-fns library for consistent date handling

import { format, formatDistanceToNow, isAfter, isBefore, parseISO } from "date-fns";

/**
 * Format a date as "Jan 15, 2024"
 * @param date - Date string or Date object
 * @returns Formatted date string
 */
export function formatDate(date: string | Date): string {
  const dateObj = typeof date === "string" ? parseISO(date) : date;
  return format(dateObj, "MMM d, yyyy");
}

/**
 * Format a date with time as "Jan 15, 2024 3:30 PM"
 * @param date - Date string or Date object
 * @returns Formatted date and time string
 */
export function formatDateTime(date: string | Date): string {
  const dateObj = typeof date === "string" ? parseISO(date) : date;
  return format(dateObj, "MMM d, yyyy h:mm a");
}

/**
 * Format a date as relative time "2 hours ago", "3 days ago"
 * @param date - Date string or Date object
 * @returns Relative time string
 */
export function formatRelativeTime(date: string | Date): string {
  const dateObj = typeof date === "string" ? parseISO(date) : date;
  return formatDistanceToNow(dateObj, { addSuffix: true });
}

/**
 * Check if a due date has passed (is overdue)
 * @param dueDate - Due date string or Date object
 * @returns true if overdue, false otherwise
 */
export function isOverdue(dueDate: string | Date): boolean {
  const dateObj = typeof dueDate === "string" ? parseISO(dueDate) : dueDate;
  return isAfter(new Date(), dateObj);
}

/**
 * Check if a date is in the future
 * @param date - Date string or Date object
 * @returns true if in future, false otherwise
 */
export function isFuture(date: string | Date): boolean {
  const dateObj = typeof date === "string" ? parseISO(date) : date;
  return isBefore(new Date(), dateObj);
}

/**
 * Format date for input fields (YYYY-MM-DD)
 * @param date - Date string or Date object
 * @returns Date string in YYYY-MM-DD format
 */
export function formatDateForInput(date: string | Date): string {
  const dateObj = typeof date === "string" ? parseISO(date) : date;
  return format(dateObj, "yyyy-MM-dd");
}

/**
 * Get a user-friendly due date label with overdue indication
 * @param dueDate - Due date string or Date object
 * @returns Object with formatted date and overdue status
 */
export function getDueDateLabel(dueDate: string | Date | null): {
  label: string;
  isOverdue: boolean;
} {
  if (!dueDate) {
    return { label: "No due date", isOverdue: false };
  }

  const dateObj = typeof dueDate === "string" ? parseISO(dueDate) : dueDate;
  const overdue = isOverdue(dateObj);

  return {
    label: formatDate(dateObj),
    isOverdue: overdue,
  };
}
