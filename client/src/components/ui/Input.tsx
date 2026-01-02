// ============================================
// Input Component
// ============================================
// Reusable input field with label, error message, and icon support
// Fully accessible with proper ARIA attributes

import React, { InputHTMLAttributes, ReactNode } from "react";

// ============================================
// Input Props
// ============================================

/**
 * Props for the Input component
 * Extends native HTML input attributes
 */
interface InputProps extends InputHTMLAttributes<HTMLInputElement> {
  label?: string; // Label text above input
  error?: string; // Error message below input
  helperText?: string; // Helper text below input
  icon?: ReactNode; // Icon to display before input text
  fullWidth?: boolean; // Take full width of container
}

// ============================================
// Input Component
// ============================================

/**
 * Input Component
 * A styled input field with label and error support
 *
 * Usage:
 * <Input
 *   label="Email"
 *   type="email"
 *   placeholder="Enter your email"
 *   error={errors.email}
 * />
 */
export default function Input({
  label,
  error,
  helperText,
  icon,
  fullWidth = true,
  className = "",
  id,
  ...props
}: InputProps) {
  // Generate unique ID if not provided
  const inputId = id || `input-${label?.replace(/\s+/g, "-").toLowerCase()}`;

  // ==========================================
  // Input Styles
  // ==========================================
  const baseStyles =
    "block rounded-lg border px-4 py-2.5 text-gray-900 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-0";

  const stateStyles = error
    ? "border-red-500 focus:border-red-500 focus:ring-red-500"
    : "border-gray-300 focus:border-primary-500 focus:ring-primary-500";

  const widthStyles = fullWidth ? "w-full" : "";

  const iconPadding = icon ? "pl-10" : "";

  const inputClasses = `${baseStyles} ${stateStyles} ${widthStyles} ${iconPadding} ${className}`;

  return (
    <div className={fullWidth ? "w-full" : ""}>
      {/* Label */}
      {label && (
        <label
          htmlFor={inputId}
          className="block text-sm font-medium text-gray-700 mb-1.5"
        >
          {label}
        </label>
      )}

      {/* Input Container (for icon positioning) */}
      <div className="relative">
        {/* Icon */}
        {icon && (
          <div className="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none text-gray-400">
            {icon}
          </div>
        )}

        {/* Input Field */}
        <input
          id={inputId}
          className={inputClasses}
          aria-invalid={error ? "true" : "false"}
          aria-describedby={
            error
              ? `${inputId}-error`
              : helperText
              ? `${inputId}-helper`
              : undefined
          }
          {...props}
        />
      </div>

      {/* Error Message */}
      {error && (
        <p
          id={`${inputId}-error`}
          className="mt-1.5 text-sm text-red-600"
          role="alert"
        >
          {error}
        </p>
      )}

      {/* Helper Text */}
      {helperText && !error && (
        <p id={`${inputId}-helper`} className="mt-1.5 text-sm text-gray-500">
          {helperText}
        </p>
      )}
    </div>
  );
}
