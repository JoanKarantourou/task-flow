// ============================================
// Select Component
// ============================================
// Reusable select dropdown with label and error support
// Matches the styling of the Input component

import { SelectHTMLAttributes } from "react";

// ============================================
// Select Props
// ============================================

interface SelectOption {
  value: string | number;
  label: string;
}

interface SelectProps extends Omit<SelectHTMLAttributes<HTMLSelectElement>, 'children'> {
  label?: string;
  error?: string;
  helperText?: string;
  options: SelectOption[];
  placeholder?: string;
  fullWidth?: boolean;
}

// ============================================
// Select Component
// ============================================

export default function Select({
  label,
  error,
  helperText,
  options,
  placeholder,
  fullWidth = true,
  className = "",
  id,
  ...props
}: SelectProps) {
  const selectId = id || `select-${label?.replace(/\s+/g, "-").toLowerCase()}`;

  const baseStyles =
    "block rounded-lg border px-4 py-2.5 text-gray-900 transition-colors duration-200 focus:outline-none focus:ring-2 focus:ring-offset-0 appearance-none bg-white cursor-pointer";

  const stateStyles = error
    ? "border-red-500 focus:border-red-500 focus:ring-red-500"
    : "border-gray-300 focus:border-primary-500 focus:ring-primary-500";

  const widthStyles = fullWidth ? "w-full" : "";

  const selectClasses = `${baseStyles} ${stateStyles} ${widthStyles} ${className}`;

  return (
    <div className={fullWidth ? "w-full" : ""}>
      {label && (
        <label
          htmlFor={selectId}
          className="block text-sm font-medium text-gray-700 mb-1.5"
        >
          {label}
        </label>
      )}

      <div className="relative">
        <select
          id={selectId}
          className={selectClasses}
          aria-invalid={error ? "true" : "false"}
          aria-describedby={
            error
              ? `${selectId}-error`
              : helperText
              ? `${selectId}-helper`
              : undefined
          }
          {...props}
        >
          {placeholder && (
            <option value="">{placeholder}</option>
          )}
          {options.map((option) => (
            <option key={option.value} value={option.value}>
              {option.label}
            </option>
          ))}
        </select>

        {/* Dropdown arrow */}
        <div className="absolute inset-y-0 right-0 flex items-center pr-3 pointer-events-none">
          <svg
            className="w-5 h-5 text-gray-400"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M19 9l-7 7-7-7"
            />
          </svg>
        </div>
      </div>

      {error && (
        <p
          id={`${selectId}-error`}
          className="mt-1.5 text-sm text-red-600"
          role="alert"
        >
          {error}
        </p>
      )}

      {helperText && !error && (
        <p id={`${selectId}-helper`} className="mt-1.5 text-sm text-gray-500">
          {helperText}
        </p>
      )}
    </div>
  );
}
