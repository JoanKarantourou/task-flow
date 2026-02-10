// ============================================
// Tasks Page - Kanban Board with Filters
// ============================================
// Display all tasks with search, filtering, and pagination

import { useState, useEffect } from "react";
import { useQuery, useMutation, useQueryClient } from "@tanstack/react-query";
import { Calendar, GripVertical, LayoutGrid, List } from "lucide-react";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import Badge from "../components/ui/Badge";
import Pagination from "../components/ui/Pagination";
import TaskFilters from "../components/tasks/TaskFilters";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import type { Task, TaskStatus, TaskPriority, TaskFilterParams } from "../types";
import toast from "react-hot-toast";

// ============================================
// Kanban Column Configuration
// ============================================

const COLUMNS: { status: TaskStatus; title: string; color: string }[] = [
  { status: 0, title: "To Do", color: "bg-gray-100" },
  { status: 1, title: "In Progress", color: "bg-blue-100" },
  { status: 2, title: "In Review", color: "bg-yellow-100" },
  { status: 3, title: "Done", color: "bg-green-100" },
];

// ============================================
// Helper Functions
// ============================================

const getPriorityBadge = (priority: TaskPriority) => {
  const priorityConfig: Record<number, { label: string; variant: "success" | "warning" | "info" | "neutral" | "danger" }> = {
    0: { label: "Low", variant: "neutral" },
    1: { label: "Medium", variant: "info" },
    2: { label: "High", variant: "warning" },
    3: { label: "Critical", variant: "danger" },
  };
  const config = priorityConfig[priority] || priorityConfig[0];
  return <Badge variant={config.variant} size="sm">{config.label}</Badge>;
};

const formatDate = (dateString?: string) => {
  if (!dateString) return null;
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
};

// ============================================
// Task Card Component
// ============================================

interface TaskCardProps {
  task: Task;
  projectName?: string;
  onDragStart: (e: React.DragEvent, task: Task) => void;
}

function TaskCard({ task, projectName, onDragStart }: TaskCardProps) {
  const dueDate = formatDate(task.dueDate);
  const isOverdue = task.dueDate && new Date(task.dueDate) < new Date() && task.status !== 3;

  return (
    <div
      draggable
      onDragStart={(e) => onDragStart(e, task)}
      className="bg-white rounded-lg shadow-sm border p-4 cursor-grab active:cursor-grabbing hover:shadow-md transition-shadow"
    >
      <div className="flex items-start gap-2">
        <GripVertical className="w-4 h-4 text-gray-300 flex-shrink-0 mt-1" />
        <div className="flex-1 min-w-0">
          <h4 className="font-medium text-gray-900 mb-2">{task.title}</h4>

          {task.description && (
            <p className="text-sm text-gray-500 mb-3 line-clamp-2">{task.description}</p>
          )}

          {projectName && (
            <div className="mb-3">
              <Badge variant="neutral" size="sm">{projectName}</Badge>
            </div>
          )}

          <div className="flex items-center justify-between">
            {getPriorityBadge(task.priority)}

            {dueDate && (
              <span className={`text-xs flex items-center gap-1 ${isOverdue ? "text-red-600" : "text-gray-500"}`}>
                <Calendar className="w-3 h-3" />
                {dueDate}
              </span>
            )}
          </div>
        </div>
      </div>
    </div>
  );
}

// ============================================
// Kanban Column Component
// ============================================

interface KanbanColumnProps {
  title: string;
  status: TaskStatus;
  tasks: Task[];
  color: string;
  projects: Map<string, string>;
  onDragStart: (e: React.DragEvent, task: Task) => void;
  onDragOver: (e: React.DragEvent) => void;
  onDrop: (e: React.DragEvent, status: TaskStatus) => void;
}

function KanbanColumn({
  title,
  status,
  tasks,
  color,
  projects,
  onDragStart,
  onDragOver,
  onDrop,
}: KanbanColumnProps) {
  return (
    <div
      className={`flex-1 min-w-[280px] rounded-lg ${color}`}
      onDragOver={onDragOver}
      onDrop={(e) => onDrop(e, status)}
    >
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h3 className="font-semibold text-gray-900">{title}</h3>
          <span className="bg-white px-2 py-0.5 rounded-full text-sm text-gray-600">
            {tasks.length}
          </span>
        </div>
      </div>

      <div className="p-4 space-y-3 min-h-[200px]">
        {tasks.length === 0 ? (
          <div className="text-center py-8 text-gray-400 text-sm">
            Drop tasks here
          </div>
        ) : (
          tasks.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              projectName={projects.get(task.projectId)}
              onDragStart={onDragStart}
            />
          ))
        )}
      </div>
    </div>
  );
}

// ============================================
// Main Tasks Page
// ============================================

export default function TasksPage() {
  const queryClient = useQueryClient();
  const [draggedTask, setDraggedTask] = useState<Task | null>(null);
  const [selectedProject, setSelectedProject] = useState<string>("all");
  const [viewMode, setViewMode] = useState<"kanban" | "list">("kanban");

  // Filter state
  const [searchTerm, setSearchTerm] = useState("");
  const [statusFilter, setStatusFilter] = useState<TaskStatus | undefined>(undefined);
  const [priorityFilter, setPriorityFilter] = useState<TaskPriority | undefined>(undefined);
  const [pageNumber, setPageNumber] = useState(1);
  const [pageSize, setPageSize] = useState(25);

  // Build filter params
  const filterParams: TaskFilterParams = {
    pageNumber,
    pageSize,
    search: searchTerm || undefined,
    status: statusFilter,
    priority: priorityFilter,
    projectId: selectedProject !== "all" ? selectedProject : undefined,
  };

  // Fetch projects
  const { data: projects = [] } = useQuery({
    queryKey: ["projects"],
    queryFn: () => projectService.getMyProjects(),
  });

  // Fetch tasks with filters
  const { data: pagedTasks, isLoading } = useQuery({
    queryKey: ["tasks", filterParams],
    queryFn: () => taskService.getAllTasks(filterParams),
  });

  const tasks = pagedTasks?.items || [];
  const totalPages = pagedTasks?.totalPages || 0;
  const totalCount = pagedTasks?.totalCount || 0;

  // Reset to page 1 when filters change
  useEffect(() => {
    setPageNumber(1);
  }, [searchTerm, statusFilter, priorityFilter, selectedProject]);

  // Update task status mutation
  const updateStatusMutation = useMutation({
    mutationFn: ({ taskId, status }: { taskId: string; status: TaskStatus }) =>
      taskService.updateTaskStatus(taskId, status),
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ["tasks"] });
      toast.success("Task status updated!");
    },
    onError: () => {
      toast.error("Failed to update task status");
    },
  });

  // Project map
  const projectMap = new Map(projects.map((p) => [p.id, p.name]));

  // Group tasks by status for Kanban view
  const tasksByStatus = COLUMNS.reduce((acc, col) => {
    acc[col.status] = tasks.filter((t) => t.status === col.status);
    return acc;
  }, {} as Record<TaskStatus, Task[]>);

  // Drag and Drop Handlers
  const handleDragStart = (e: React.DragEvent, task: Task) => {
    setDraggedTask(task);
    e.dataTransfer.effectAllowed = "move";
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    e.dataTransfer.dropEffect = "move";
  };

  const handleDrop = async (e: React.DragEvent, newStatus: TaskStatus) => {
    e.preventDefault();

    if (!draggedTask || draggedTask.status === newStatus) {
      setDraggedTask(null);
      return;
    }

    updateStatusMutation.mutate({ taskId: draggedTask.id, status: newStatus });
    setDraggedTask(null);
  };

  const handleClearFilters = () => {
    setSearchTerm("");
    setStatusFilter(undefined);
    setPriorityFilter(undefined);
    setSelectedProject("all");
    setPageNumber(1);
  };

  const handlePageSizeChange = (newSize: number) => {
    setPageSize(newSize);
    setPageNumber(1);
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="h-full flex flex-col">
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">All Tasks</h1>
          <p className="text-gray-600 mt-1">
            {totalCount} total tasks
            {viewMode === "kanban" && " - Drag and drop to change status"}
          </p>
        </div>

        <div className="flex items-center gap-4">
          {/* View Toggle */}
          <div className="flex bg-gray-100 rounded-lg p-1">
            <button
              onClick={() => setViewMode("kanban")}
              className={`p-2 rounded ${viewMode === "kanban" ? "bg-white shadow-sm" : ""}`}
              aria-label="Kanban view"
            >
              <LayoutGrid className="w-5 h-5" />
            </button>
            <button
              onClick={() => setViewMode("list")}
              className={`p-2 rounded ${viewMode === "list" ? "bg-white shadow-sm" : ""}`}
              aria-label="List view"
            >
              <List className="w-5 h-5" />
            </button>
          </div>

          {/* Project Filter */}
          <select
            value={selectedProject}
            onChange={(e) => setSelectedProject(e.target.value)}
            className="rounded-lg border border-gray-300 px-4 py-2 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500"
          >
            <option value="all">All Projects</option>
            {projects.map((project) => (
              <option key={project.id} value={project.id}>
                {project.name}
              </option>
            ))}
          </select>
        </div>
      </div>

      {/* Filters */}
      <div className="mb-6">
        <TaskFilters
          searchTerm={searchTerm}
          onSearchChange={setSearchTerm}
          status={statusFilter}
          onStatusChange={setStatusFilter}
          priority={priorityFilter}
          onPriorityChange={setPriorityFilter}
          onClearFilters={handleClearFilters}
        />
      </div>

      {/* Task Stats (Kanban view only) */}
      {viewMode === "kanban" && (
        <div className="grid grid-cols-4 gap-4 mb-6">
          {COLUMNS.map((col) => (
            <Card key={col.status} padding="sm">
              <div className="text-sm text-gray-500">{col.title}</div>
              <div className="text-2xl font-bold text-gray-900">
                {tasksByStatus[col.status]?.length || 0}
              </div>
            </Card>
          ))}
        </div>
      )}

      {/* Content */}
      {tasks.length === 0 ? (
        <Card className="text-center py-12">
          <h3 className="text-lg font-medium text-gray-900 mb-2">No tasks found</h3>
          <p className="text-gray-600 mb-4">
            {searchTerm || statusFilter !== undefined || priorityFilter !== undefined
              ? "Try adjusting your filters"
              : "Create tasks in your projects to see them here"}
          </p>
          {(searchTerm || statusFilter !== undefined || priorityFilter !== undefined) && (
            <Button variant="secondary" onClick={handleClearFilters}>
              Clear Filters
            </Button>
          )}
        </Card>
      ) : viewMode === "kanban" ? (
        /* Kanban Board */
        <div className="flex gap-4 overflow-x-auto pb-4 flex-1">
          {COLUMNS.map((col) => (
            <KanbanColumn
              key={col.status}
              title={col.title}
              status={col.status}
              tasks={tasksByStatus[col.status] || []}
              color={col.color}
              projects={projectMap}
              onDragStart={handleDragStart}
              onDragOver={handleDragOver}
              onDrop={handleDrop}
            />
          ))}
        </div>
      ) : (
        /* List View */
        <div className="flex-1">
          <Card>
            <div className="space-y-2">
              {tasks.map((task) => (
                <div
                  key={task.id}
                  className="flex items-center justify-between p-4 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors"
                >
                  <div className="flex items-center gap-4 flex-1 min-w-0">
                    <div className="flex-1 min-w-0">
                      <h4 className="font-medium text-gray-900 truncate">{task.title}</h4>
                      {task.description && (
                        <p className="text-sm text-gray-500 truncate">{task.description}</p>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-4">
                    <Badge variant="neutral" size="sm">
                      {projectMap.get(task.projectId) || "Unknown"}
                    </Badge>
                    {getPriorityBadge(task.priority)}
                    <Badge
                      variant={
                        task.status === 3
                          ? "success"
                          : task.status === 1
                          ? "warning"
                          : "info"
                      }
                      size="sm"
                    >
                      {COLUMNS.find((c) => c.status === task.status)?.title || "Unknown"}
                    </Badge>
                    {task.dueDate && (
                      <span className="text-xs text-gray-500 flex items-center gap-1">
                        <Calendar className="w-3 h-3" />
                        {formatDate(task.dueDate)}
                      </span>
                    )}
                  </div>
                </div>
              ))}
            </div>
          </Card>

          {/* Pagination */}
          <div className="mt-4">
            <Pagination
              currentPage={pageNumber}
              totalPages={totalPages}
              onPageChange={setPageNumber}
              pageSize={pageSize}
              onPageSizeChange={handlePageSizeChange}
              totalItems={totalCount}
            />
          </div>
        </div>
      )}
    </div>
  );
}
