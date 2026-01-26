// ============================================
// Button Component
// ============================================
// Reusable button with multiple variants and sizes
// Supports icons, loading states, and full accessibility

import { ButtonHTMLAttributes, ReactNode } from "react";

// ============================================
// Button Props
// ============================================

/**
 * Props for the Button component
 * Extends native HTML button attributes for full compatibility
 */
interface ButtonProps extends ButtonHTMLAttributes<HTMLButtonElement> {
  children: ReactNode; // Button content (text, icons, etc.)
  variant?: "primary" | "secondary" | "danger" | "ghost"; // Visual style
  size?: "sm" | "md" | "lg"; // Button size
  isLoading?: boolean; // Show loading spinner
  fullWidth?: boolean; // Take full width of container
  icon?: ReactNode; // Optional icon before text
}

// ============================================
// Button Component
// ============================================

/**
 * Button Component
 * A flexible, accessible button with multiple variants
 *
 * Usage:
 * <Button variant="primary" size="md" onClick={handleClick}>
 *   Click Me
 * </Button>
 */
export default function Button({
  children,
  variant = "primary",
  size = "md",
  isLoading = false,
  fullWidth = false,
  icon,
  disabled,
  className = "",
  ...props
}: ButtonProps) {
  // ==========================================
  // Base Styles (Always Applied)
  // ==========================================
  const baseStyles =
    "inline-flex items-center justify-center font-medium rounded-lg transition-all duration-200 focus:outline-none focus:ring-2 focus:ring-offset-2 disabled:opacity-50 disabled:cursor-not-allowed";

  // ==========================================
  // Variant Styles
  // ==========================================
  const variantStyles = {
    primary:
      "bg-primary-600 text-white hover:bg-primary-700 focus:ring-primary-500 active:bg-primary-800",
    secondary:
      "bg-gray-200 text-gray-800 hover:bg-gray-300 focus:ring-gray-500 active:bg-gray-400",
    danger:
      "bg-red-600 text-white hover:bg-red-700 focus:ring-red-500 active:bg-red-800",
    ghost: "bg-transparent text-gray-700 hover:bg-gray-100 focus:ring-gray-500",
  };

  // ==========================================
  // Size Styles
  // ==========================================
  const sizeStyles = {
    sm: "px-3 py-1.5 text-sm",
    md: "px-4 py-2 text-base",
    lg: "px-6 py-3 text-lg",
  };

  // ==========================================
  // Width Styles
  // ==========================================
  const widthStyles = fullWidth ? "w-full" : "";

  // ==========================================
  // Combined Classes
  // ==========================================
  const buttonClasses = `${baseStyles} ${variantStyles[variant]} ${sizeStyles[size]} ${widthStyles} ${className}`;

  return (
    <button
      className={buttonClasses}
      disabled={disabled || isLoading}
      {...props}
    >
      {/* Loading Spinner */}
      {isLoading && (
        <svg
          className="animate-spin -ml-1 mr-2 h-4 w-4"
          xmlns="http://www.w3.org/2000/svg"
          fill="none"
          viewBox="0 0 24 24"
        >
          <circle
            className="opacity-25"
            cx="12"
            cy="12"
            r="10"
            stroke="currentColor"
            strokeWidth="4"
          />
          <path
            className="opacity-75"
            fill="currentColor"
            d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
          />
        </svg>
      )}

      {/* Icon */}
      {icon && !isLoading && <span className="mr-2">{icon}</span>}

      {/* Button Text */}
      {children}
    </button>
  );
}
