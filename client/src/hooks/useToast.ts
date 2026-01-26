// ============================================
// useToast Hook
// ============================================
// Custom React hook for toast notifications
// Wrapper around react-hot-toast for consistent usage

import toast from "react-hot-toast";

/**
 * Toast notification hook
 * Provides convenient methods for showing toast notifications
 * Uses react-hot-toast under the hood
 *
 * @returns Object with toast notification methods
 *
 * Usage:
 * const showToast = useToast();
 * showToast.success('Task created successfully!');
 * showToast.error('Failed to create task');
 */
export function useToast() {
  return {
    /**
     * Show success toast notification
     * @param message - Success message to display
     */
    success: (message: string) => {
      toast.success(message);
    },

    /**
     * Show error toast notification
     * @param message - Error message to display
     */
    error: (message: string) => {
      toast.error(message);
    },

    /**
     * Show info toast notification
     * @param message - Info message to display
     */
    info: (message: string) => {
      toast(message, {
        icon: "ℹ️",
      });
    },

    /**
     * Show loading toast notification
     * Returns a toast ID that can be used to dismiss it later
     * @param message - Loading message to display
     * @returns Toast ID
     */
    loading: (message: string) => {
      return toast.loading(message);
    },

    /**
     * Dismiss a specific toast by ID
     * @param toastId - ID of the toast to dismiss
     */
    dismiss: (toastId: string) => {
      toast.dismiss(toastId);
    },

    /**
     * Dismiss all toasts
     */
    dismissAll: () => {
      toast.dismiss();
    },

    /**
     * Show a promise-based toast
     * Automatically shows loading, then success or error based on promise
     * @param promise - Promise to track
     * @param messages - Messages for loading, success, and error states
     */
    promise: <T,>(
      promise: Promise<T>,
      messages: {
        loading: string;
        success: string;
        error: string;
      }
    ) => {
      return toast.promise(promise, messages);
    },
  };
}

export default useToast;
