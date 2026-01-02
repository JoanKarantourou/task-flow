// ============================================
// Main Layout Component
// ============================================
// Combines Header and Sidebar to create the main app layout
// Wraps all authenticated pages

import React, { ReactNode } from "react";
import Header from "./Header";
import Sidebar from "./Sidebar";

// ============================================
// MainLayout Props
// ============================================

/**
 * Props for MainLayout component
 */
interface MainLayoutProps {
  children: ReactNode; // Page content
}

// ============================================
// MainLayout Component
// ============================================

/**
 * Main Layout Component
 * Provides the standard layout for authenticated pages
 *
 * Layout structure:
 * ┌─────────────────────────────┐
 * │         Header              │
 * ├──────────┬──────────────────┤
 * │          │                  │
 * │ Sidebar  │   Page Content   │
 * │          │                  │
 * │          │                  │
 * └──────────┴──────────────────┘
 *
 * Usage:
 * <MainLayout>
 *   <DashboardPage />
 * </MainLayout>
 */
export default function MainLayout({ children }: MainLayoutProps) {
  return (
    <div className="flex h-screen overflow-hidden bg-gray-100">
      {/* Sidebar - Fixed on the left */}
      <Sidebar />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col overflow-hidden">
        {/* Header - Sticky at top */}
        <Header />

        {/* Page Content - Scrollable */}
        <main className="flex-1 overflow-y-auto p-6">
          {/* Page content goes here */}
          {children}
        </main>
      </div>
    </div>
  );
}
