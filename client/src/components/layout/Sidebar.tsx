// ============================================
// Sidebar Component
// ============================================
// Left navigation menu with links to different pages
// Collapsible on mobile devices

import React, { useState } from "react";
import { NavLink } from "react-router-dom";

// ============================================
// Navigation Item Interface
// ============================================

/**
 * Navigation item structure
 */
interface NavItem {
  name: string;
  href: string;
  icon: React.ReactNode;
  badge?: number; // Optional badge count (e.g., for notifications)
}

// ============================================
// Sidebar Component
// ============================================

/**
 * Sidebar Component
 * Navigation menu with icons and labels
 */
export default function Sidebar() {
  const [isCollapsed, setIsCollapsed] = useState(false);

  // ==========================================
  // Navigation Items
  // ==========================================

  /**
   * Define navigation menu items
   * Updated with React Router paths
   */
  const navItems: NavItem[] = [
    {
      name: "Dashboard",
      href: "/dashboard",
      icon: (
        <svg
          className="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M3 12l2-2m0 0l7-7 7 7M5 10v10a1 1 0 001 1h3m10-11l2 2m-2-2v10a1 1 0 01-1 1h-3m-6 0a1 1 0 001-1v-4a1 1 0 011-1h2a1 1 0 011 1v4a1 1 0 001 1m-6 0h6"
          />
        </svg>
      ),
    },
    {
      name: "Projects",
      href: "/projects",
      icon: (
        <svg
          className="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M3 7v10a2 2 0 002 2h14a2 2 0 002-2V9a2 2 0 00-2-2h-6l-2-2H5a2 2 0 00-2 2z"
          />
        </svg>
      ),
    },
    {
      name: "Tasks",
      href: "/tasks",
      icon: (
        <svg
          className="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2m-6 9l2 2 4-4"
          />
        </svg>
      ),
      badge: 5, // Example: 5 pending tasks
    },
    {
      name: "Profile",
      href: "/profile",
      icon: (
        <svg
          className="w-5 h-5"
          fill="none"
          stroke="currentColor"
          viewBox="0 0 24 24"
        >
          <path
            strokeLinecap="round"
            strokeLinejoin="round"
            strokeWidth={2}
            d="M16 7a4 4 0 11-8 0 4 4 0 018 0zM12 14a7 7 0 00-7 7h14a7 7 0 00-7-7z"
          />
        </svg>
      ),
    },
  ];

  // ==========================================
  // Event Handlers
  // ==========================================

  /**
   * Toggle sidebar collapsed state
   */
  const toggleSidebar = () => {
    setIsCollapsed(!isCollapsed);
  };

  return (
    <aside
      className={`bg-gray-900 text-white transition-all duration-300 ${
        isCollapsed ? "w-20" : "w-64"
      } flex flex-col h-screen sticky top-0`}
    >
      {/* Sidebar Header */}
      <div className="p-4 flex items-center justify-between border-b border-gray-800">
        {!isCollapsed && (
          <span className="text-lg font-semibold">Navigation</span>
        )}

        {/* Collapse Button */}
        <button
          onClick={toggleSidebar}
          className="p-2 rounded-lg hover:bg-gray-800 transition-colors"
          aria-label={isCollapsed ? "Expand sidebar" : "Collapse sidebar"}
        >
          <svg
            className={`w-5 h-5 transition-transform ${
              isCollapsed ? "rotate-180" : ""
            }`}
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M11 19l-7-7 7-7m8 14l-7-7 7-7"
            />
          </svg>
        </button>
      </div>

      {/* Navigation Items */}
      <nav className="flex-1 p-4 space-y-2 overflow-y-auto">
        {navItems.map((item) => (
          <NavLink
            key={item.name}
            to={item.href}
            className={({ isActive }) =>
              `flex items-center gap-3 px-3 py-3 rounded-lg transition-colors group relative ${
                isActive
                  ? "bg-primary-600 text-white"
                  : "text-gray-300 hover:bg-gray-800 hover:text-white"
              }`
            }
            title={isCollapsed ? item.name : undefined}
          >
            {/* Icon */}
            <span className="flex-shrink-0">{item.icon}</span>

            {/* Label (hidden when collapsed) */}
            {!isCollapsed && (
              <>
                <span className="flex-1 text-sm font-medium">{item.name}</span>

                {/* Badge (if present) */}
                {item.badge !== undefined && (
                  <span className="px-2 py-0.5 text-xs font-semibold bg-white/20 text-white rounded-full">
                    {item.badge}
                  </span>
                )}
              </>
            )}

            {/* Tooltip for collapsed state */}
            {isCollapsed && (
              <span className="absolute left-full ml-2 px-2 py-1 bg-gray-800 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap z-50 transition-opacity">
                {item.name}
                {item.badge !== undefined && ` (${item.badge})`}
              </span>
            )}
          </NavLink>
        ))}
      </nav>

      {/* Sidebar Footer */}
      <div className="p-4 border-t border-gray-800">
        <a
          href="#"
          className="flex items-center gap-3 px-3 py-3 rounded-lg text-gray-300 hover:bg-gray-800 hover:text-white transition-colors group relative"
          title={isCollapsed ? "Help & Support" : undefined}
        >
          {/* Help Icon */}
          <svg
            className="w-5 h-5 flex-shrink-0"
            fill="none"
            stroke="currentColor"
            viewBox="0 0 24 24"
          >
            <path
              strokeLinecap="round"
              strokeLinejoin="round"
              strokeWidth={2}
              d="M8.228 9c.549-1.165 2.03-2 3.772-2 2.21 0 4 1.343 4 3 0 1.4-1.278 2.575-3.006 2.907-.542.104-.994.54-.994 1.093m0 3h.01M21 12a9 9 0 11-18 0 9 9 0 0118 0z"
            />
          </svg>

          {/* Label */}
          {!isCollapsed && (
            <span className="text-sm font-medium">Help & Support</span>
          )}

          {/* Tooltip for collapsed state */}
          {isCollapsed && (
            <span className="absolute left-full ml-2 px-2 py-1 bg-gray-800 text-white text-sm rounded-lg opacity-0 group-hover:opacity-100 pointer-events-none whitespace-nowrap z-50">
              Help & Support
            </span>
          )}
        </a>
      </div>
    </aside>
  );
}
