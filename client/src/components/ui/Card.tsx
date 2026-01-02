// ============================================
// Card Component
// ============================================
// Container component for grouping related content
// Provides consistent spacing and styling

import React, { ReactNode } from "react";

// ============================================
// Card Props
// ============================================

/**
 * Props for the Card component
 */
interface CardProps {
  children: ReactNode; // Card content
  title?: string; // Optional title at top of card
  className?: string; // Additional CSS classes
  padding?: "none" | "sm" | "md" | "lg"; // Padding size
  hoverable?: boolean; // Add hover effect
  onClick?: () => void; // Make card clickable
}

// ============================================
// Card Component
// ============================================

/**
 * Card Component
 * A container with shadow and rounded corners
 *
 * Usage:
 * <Card title="My Card">
 *   <p>Card content here</p>
 * </Card>
 */
export default function Card({
  children,
  title,
  className = "",
  padding = "md",
  hoverable = false,
  onClick,
}: CardProps) {
  // ==========================================
  // Base Styles
  // ==========================================
  const baseStyles =
    "bg-white rounded-lg shadow-md transition-shadow duration-200";

  // ==========================================
  // Padding Styles
  // ==========================================
  const paddingStyles = {
    none: "",
    sm: "p-4",
    md: "p-6",
    lg: "p-8",
  };

  // ==========================================
  // Interactive Styles
  // ==========================================
  const hoverStyles = hoverable ? "hover:shadow-lg cursor-pointer" : "";
  const clickableStyles = onClick ? "cursor-pointer" : "";

  // ==========================================
  // Combined Classes
  // ==========================================
  const cardClasses = `${baseStyles} ${paddingStyles[padding]} ${hoverStyles} ${clickableStyles} ${className}`;

  return (
    <div className={cardClasses} onClick={onClick}>
      {/* Card Title */}
      {title && (
        <h3 className="text-lg font-semibold text-gray-900 mb-4">{title}</h3>
      )}

      {/* Card Content */}
      {children}
    </div>
  );
}
