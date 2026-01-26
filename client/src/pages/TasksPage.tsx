// ============================================
// Tasks Page - Kanban Board
// ============================================
// Display all tasks across projects in a Kanban board view

import { useState, useEffect } from "react";
import { Calendar, GripVertical } from "lucide-react";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import Badge from "../components/ui/Badge";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import type { Task, Project, TaskStatus, TaskPriority } from "../types";
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
      {/* Drag Handle */}
      <div className="flex items-start gap-2">
        <GripVertical className="w-4 h-4 text-gray-300 flex-shrink-0 mt-1" />
        <div className="flex-1 min-w-0">
          {/* Task Title */}
          <h4 className="font-medium text-gray-900 mb-2">{task.title}</h4>

          {/* Description */}
          {task.description && (
            <p className="text-sm text-gray-500 mb-3 line-clamp-2">{task.description}</p>
          )}

          {/* Project Badge */}
          {projectName && (
            <div className="mb-3">
              <Badge variant="neutral" size="sm">{projectName}</Badge>
            </div>
          )}

          {/* Footer */}
          <div className="flex items-center justify-between">
            {/* Priority */}
            {getPriorityBadge(task.priority)}

            {/* Due Date */}
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
      {/* Column Header */}
      <div className="p-4 border-b border-gray-200">
        <div className="flex items-center justify-between">
          <h3 className="font-semibold text-gray-900">{title}</h3>
          <span className="bg-white px-2 py-0.5 rounded-full text-sm text-gray-600">
            {tasks.length}
          </span>
        </div>
      </div>

      {/* Tasks */}
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
  const [tasks, setTasks] = useState<Task[]>([]);
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [draggedTask, setDraggedTask] = useState<Task | null>(null);
  const [selectedProject, setSelectedProject] = useState<string>("all");

  useEffect(() => {
    fetchData();
  }, []);

  const fetchData = async () => {
    try {
      setIsLoading(true);
      const projectsData = await projectService.getMyProjects();
      setProjects(projectsData);

      // Fetch tasks for all projects
      const allTasks: Task[] = [];
      for (const project of projectsData) {
        try {
          const projectTasks = await taskService.getTasksByProjectId(project.id);
          allTasks.push(...projectTasks);
        } catch (error) {
          console.error(`Failed to fetch tasks for project ${project.id}:`, error);
        }
      }
      setTasks(allTasks);
    } catch (error) {
      console.error("Failed to fetch data:", error);
      toast.error("Failed to load tasks");
    } finally {
      setIsLoading(false);
    }
  };

  // Create a map of project IDs to names
  const projectMap = new Map(projects.map((p) => [p.id, p.name]));

  // Filter tasks by selected project
  const filteredTasks = selectedProject === "all"
    ? tasks
    : tasks.filter((t) => t.projectId === selectedProject);

  // Group tasks by status
  const tasksByStatus = COLUMNS.reduce((acc, col) => {
    acc[col.status] = filteredTasks.filter((t) => t.status === col.status);
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

    try {
      // Optimistic update
      setTasks(tasks.map((t) =>
        t.id === draggedTask.id ? { ...t, status: newStatus } : t
      ));

      // API call
      await taskService.updateTaskStatus(draggedTask.id, newStatus);
      toast.success("Task status updated!");
    } catch (error) {
      console.error("Failed to update task status:", error);
      // Revert on error
      setTasks(tasks.map((t) =>
        t.id === draggedTask.id ? { ...t, status: draggedTask.status } : t
      ));
      toast.error("Failed to update task status");
    } finally {
      setDraggedTask(null);
    }
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
          <p className="text-gray-600 mt-1">Drag and drop tasks to change their status</p>
        </div>

        {/* Project Filter */}
        <div className="flex items-center gap-4">
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

      {/* Stats */}
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

      {/* Kanban Board */}
      {tasks.length === 0 ? (
        <Card className="text-center py-12">
          <h3 className="text-lg font-medium text-gray-900 mb-2">No tasks found</h3>
          <p className="text-gray-600 mb-4">
            Create tasks in your projects to see them here
          </p>
          <Button onClick={() => window.location.href = "/projects"}>
            Go to Projects
          </Button>
        </Card>
      ) : (
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
      )}
    </div>
  );
}
