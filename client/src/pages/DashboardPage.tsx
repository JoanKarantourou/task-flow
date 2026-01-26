// ============================================
// Dashboard Page
// ============================================
// Main dashboard showing overview of projects and tasks
// Uses React Query for data fetching

import { useQuery } from "@tanstack/react-query";
import { useAuth } from "../context/AuthContext";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import Card from "../components/ui/Card";
import Badge from "../components/ui/Badge";
import Button from "../components/ui/Button";
import { ProjectStatus, TaskStatus } from "../types";

export default function DashboardPage() {
  const { user } = useAuth();

  // Fetch projects
  const { data: projects = [], isLoading: isLoadingProjects } = useQuery({
    queryKey: ["projects"],
    queryFn: () => projectService.getMyProjects(),
  });

  // Fetch tasks (will show error in console until backend endpoint exists)
  const { data: allTasks = [] } = useQuery({
    queryKey: ["tasks"],
    queryFn: () => taskService.getAllTasks(),
    retry: 0, // Don't retry if endpoint doesn't exist yet
  });

  // Calculate stats
  const activeProjects = projects.filter(
    (p) => p.status === ProjectStatus.Active
  ).length;

  const pendingTasks = allTasks.filter(
    (t) =>
      t.status === TaskStatus.Todo || t.status === TaskStatus.InProgress
  ).length;

  const completedTasks = allTasks.filter(
    (t) => t.status === TaskStatus.Done
  ).length;

  // Get recent tasks (last 10)
  const recentTasks = allTasks.slice(0, 10);

  return (
    <div className="space-y-6">
      {/* Welcome Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">
          Welcome back, {user?.firstName}! 👋
        </h1>
        <p className="text-gray-600 mt-1">
          Here's what's happening with your projects today.
        </p>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Active Projects</p>
              <p className="text-3xl font-bold text-gray-900 mt-1">
                {isLoadingProjects ? "..." : activeProjects}
              </p>
            </div>
            <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center">
              <svg
                className="w-6 h-6 text-blue-600"
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
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Pending Tasks</p>
              <p className="text-3xl font-bold text-gray-900 mt-1">
                {pendingTasks}
              </p>
            </div>
            <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center">
              <svg
                className="w-6 h-6 text-yellow-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 5H7a2 2 0 00-2 2v12a2 2 0 002 2h10a2 2 0 002-2V7a2 2 0 00-2-2h-2M9 5a2 2 0 002 2h2a2 2 0 002-2M9 5a2 2 0 012-2h2a2 2 0 012 2"
                />
              </svg>
            </div>
          </div>
        </Card>

        <Card>
          <div className="flex items-center justify-between">
            <div>
              <p className="text-sm text-gray-600">Completed</p>
              <p className="text-3xl font-bold text-gray-900 mt-1">
                {completedTasks}
              </p>
            </div>
            <div className="w-12 h-12 bg-green-100 rounded-lg flex items-center justify-center">
              <svg
                className="w-6 h-6 text-green-600"
                fill="none"
                stroke="currentColor"
                viewBox="0 0 24 24"
              >
                <path
                  strokeLinecap="round"
                  strokeLinejoin="round"
                  strokeWidth={2}
                  d="M9 12l2 2 4-4m6 2a9 9 0 11-18 0 9 9 0 0118 0z"
                />
              </svg>
            </div>
          </div>
        </Card>
      </div>

      {/* Recent Tasks */}
      <Card title="Recent Tasks">
        {recentTasks.length > 0 ? (
          <div className="space-y-3">
            {recentTasks.map((task) => (
              <div
                key={task.id}
                className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
              >
                <div className="flex items-center gap-3">
                  <input
                    type="checkbox"
                    className="w-4 h-4 rounded"
                    checked={task.status === TaskStatus.Done}
                    readOnly
                  />
                  <span
                    className={`text-gray-900 ${
                      task.status === TaskStatus.Done ? "line-through" : ""
                    }`}
                  >
                    {task.title}
                  </span>
                </div>
                <Badge
                  variant={
                    task.status === TaskStatus.Done
                      ? "success"
                      : task.status === TaskStatus.InProgress
                      ? "warning"
                      : "info"
                  }
                >
                  {task.status === TaskStatus.Done
                    ? "Done"
                    : task.status === TaskStatus.InProgress
                    ? "In Progress"
                    : task.status === TaskStatus.InReview
                    ? "In Review"
                    : "To Do"}
                </Badge>
              </div>
            ))}
          </div>
        ) : (
          <div className="text-center py-8 text-gray-500">
            <p>No tasks yet. Create your first task to get started!</p>
          </div>
        )}

        <div className="mt-4">
          <Button variant="ghost" fullWidth>
            View All Tasks →
          </Button>
        </div>
      </Card>
    </div>
  );
}
