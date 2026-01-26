// ============================================
// Avatar Utilities
// ============================================
// Helper functions for generating user avatars
// Generates initials and consistent colors based on user data

/**
 * Get user initials from first and last name
 * @param firstName - User's first name
 * @param lastName - User's last name
 * @returns Initials (e.g., "JK" for "Joan Karantourou")
 */
export function getInitials(firstName: string, lastName: string): string {
  const firstInitial = firstName?.charAt(0)?.toUpperCase() || "";
  const lastInitial = lastName?.charAt(0)?.toUpperCase() || "";
  return `${firstInitial}${lastInitial}`;
}

/**
 * Get consistent avatar background color based on user ID
 * Uses a hash function to ensure same user always gets same color
 * @param userId - User's unique identifier
 * @returns Tailwind CSS background color class
 */
export function getAvatarColor(userId: string): string {
  // Array of pleasant avatar colors
  const colors = [
    "bg-red-500",
    "bg-orange-500",
    "bg-amber-500",
    "bg-yellow-500",
    "bg-lime-500",
    "bg-green-500",
    "bg-emerald-500",
    "bg-teal-500",
    "bg-cyan-500",
    "bg-sky-500",
    "bg-blue-500",
    "bg-indigo-500",
    "bg-violet-500",
    "bg-purple-500",
    "bg-fuchsia-500",
    "bg-pink-500",
    "bg-rose-500",
  ];

  // Simple hash function to get consistent index
  const hash = userId
    .split("")
    .reduce((acc, char) => acc + char.charCodeAt(0), 0);

  const index = hash % colors.length;
  return colors[index];
}

/**
 * Get avatar initials and color together
 * @param firstName - User's first name
 * @param lastName - User's last name
 * @param userId - User's unique identifier
 * @returns Object with initials and color class
 */
export function getAvatarData(
  firstName: string,
  lastName: string,
  userId: string
): {
  initials: string;
  colorClass: string;
} {
  return {
    initials: getInitials(firstName, lastName),
    colorClass: getAvatarColor(userId),
  };
}

/**
 * Get text color (white or black) based on background color for accessibility
 * @param bgColorClass - Tailwind background color class (e.g., "bg-red-500")
 * @returns Tailwind text color class (text-white or text-gray-900)
 */
export function getAvatarTextColor(bgColorClass: string): string {
  // For 500-level colors, white text is generally better
  // For lighter colors (100-300), use dark text
  if (bgColorClass.includes("-500") || bgColorClass.includes("-600")) {
    return "text-white";
  }
  return "text-gray-900";
}
