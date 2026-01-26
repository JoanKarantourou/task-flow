// ============================================
// Badge Component
// ============================================
// Small colored labels for status indicators, tags, counts, etc.
// Perfect for task status, priority levels, notifications

import { ReactNode } from "react";

// ============================================
// Badge Props
// ============================================

/**
 * Props for the Badge component
 */
interface BadgeProps {
  children: ReactNode; // Badge content
  variant?: "success" | "warning" | "danger" | "info" | "neutral"; // Color scheme
  size?: "sm" | "md" | "lg"; // Badge size
  rounded?: boolean; // Fully rounded corners
  className?: string; // Additional CSS classes
}

// ============================================
// Badge Component
// ============================================

/**
 * Badge Component
 * A small colored label for displaying status or categories
 *
 * Usage:
 * <Badge variant="success">Completed</Badge>
 * <Badge variant="warning">In Progress</Badge>
 */
export default function Badge({
  children,
  variant = "neutral",
  size = "md",
  rounded = false,
  className = "",
}: BadgeProps) {
  // ==========================================
  // Base Styles
  // ==========================================
  const baseStyles = "inline-flex items-center font-medium";

  // ==========================================
  // Variant Styles (Colors)
  // ==========================================
  const variantStyles = {
    success: "bg-green-100 text-green-800",
    warning: "bg-yellow-100 text-yellow-800",
    danger: "bg-red-100 text-red-800",
    info: "bg-blue-100 text-blue-800",
    neutral: "bg-gray-100 text-gray-800",
  };

  // ==========================================
  // Size Styles
  // ==========================================
  const sizeStyles = {
    sm: "px-2 py-0.5 text-xs",
    md: "px-2.5 py-1 text-sm",
    lg: "px-3 py-1.5 text-base",
  };

  // ==========================================
  // Border Radius
  // ==========================================
  const roundedStyles = rounded ? "rounded-full" : "rounded";

  // ==========================================
  // Combined Classes
  // ==========================================
  const badgeClasses = `${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${roundedStyles} ${className}`;

  return <span className={badgeClasses}>{children}</span>;
}
