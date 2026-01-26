// ============================================
// Project Detail Page
// ============================================
// Display single project with tasks and project info

import { useState, useEffect } from "react";
import { useParams, useNavigate } from "react-router-dom";
import {
  ArrowLeft,
  Plus,
  Calendar,
  Clock,
  CheckCircle2,
  Circle,
  AlertCircle,
  MoreVertical,
  Pencil,
  Trash2,
} from "lucide-react";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import Modal from "../components/ui/Modal";
import Input from "../components/ui/Input";
import Badge from "../components/ui/Badge";
import projectService from "../services/projectService";
import taskService from "../services/taskService";
import type { Project, Task, CreateTaskRequest, TaskStatus, TaskPriority } from "../types";
import toast from "react-hot-toast";

// ============================================
// Helper Functions
// ============================================

const getStatusBadge = (status: TaskStatus) => {
  const statusConfig: Record<number, { label: string; variant: "success" | "warning" | "info" | "neutral" | "danger" }> = {
    0: { label: "To Do", variant: "neutral" },
    1: { label: "In Progress", variant: "info" },
    2: { label: "In Review", variant: "warning" },
    3: { label: "Done", variant: "success" },
    4: { label: "Cancelled", variant: "danger" },
  };
  const config = statusConfig[status] || statusConfig[0];
  return <Badge variant={config.variant}>{config.label}</Badge>;
};

const getPriorityBadge = (priority: TaskPriority) => {
  const priorityConfig: Record<number, { label: string; variant: "success" | "warning" | "info" | "neutral" | "danger" }> = {
    0: { label: "Low", variant: "neutral" },
    1: { label: "Medium", variant: "info" },
    2: { label: "High", variant: "warning" },
    3: { label: "Critical", variant: "danger" },
  };
  const config = priorityConfig[priority] || priorityConfig[0];
  return <Badge variant={config.variant}>{config.label}</Badge>;
};

const getStatusIcon = (status: TaskStatus) => {
  switch (status) {
    case 3: return <CheckCircle2 className="w-5 h-5 text-green-500" />;
    case 1:
    case 2: return <Clock className="w-5 h-5 text-blue-500" />;
    case 4: return <AlertCircle className="w-5 h-5 text-red-500" />;
    default: return <Circle className="w-5 h-5 text-gray-400" />;
  }
};

const formatDate = (dateString?: string) => {
  if (!dateString) return "No date";
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
  });
};

// ============================================
// Task Item Component
// ============================================

interface TaskItemProps {
  task: Task;
  onEdit: (task: Task) => void;
  onDelete: (task: Task) => void;
  onStatusChange: (task: Task, status: TaskStatus) => void;
}

function TaskItem({ task, onEdit, onDelete, onStatusChange }: TaskItemProps) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <div className="flex items-center gap-4 p-4 bg-white rounded-lg border hover:shadow-sm transition-shadow">
      {/* Status Icon */}
      <button
        onClick={() => {
          const nextStatus = task.status === 3 ? 0 : task.status + 1;
          onStatusChange(task, nextStatus as TaskStatus);
        }}
        className="flex-shrink-0"
      >
        {getStatusIcon(task.status)}
      </button>

      {/* Task Info */}
      <div className="flex-1 min-w-0">
        <div className="flex items-center gap-2 mb-1">
          <h4 className={`font-medium ${task.status === 3 ? "line-through text-gray-400" : "text-gray-900"}`}>
            {task.title}
          </h4>
        </div>
        {task.description && (
          <p className="text-sm text-gray-500 truncate">{task.description}</p>
        )}
      </div>

      {/* Badges */}
      <div className="flex items-center gap-2 flex-shrink-0">
        {getStatusBadge(task.status)}
        {getPriorityBadge(task.priority)}
        {task.dueDate && (
          <span className="text-sm text-gray-500 flex items-center gap-1">
            <Calendar className="w-4 h-4" />
            {formatDate(task.dueDate)}
          </span>
        )}
      </div>

      {/* Menu */}
      <div className="relative">
        <button
          onClick={() => setShowMenu(!showMenu)}
          className="p-1 rounded hover:bg-gray-100"
        >
          <MoreVertical className="w-5 h-5 text-gray-400" />
        </button>
        {showMenu && (
          <div className="absolute right-0 mt-1 w-36 bg-white rounded-lg shadow-lg border z-10">
            <button
              onClick={() => {
                onEdit(task);
                setShowMenu(false);
              }}
              className="w-full px-4 py-2 text-left text-sm hover:bg-gray-100 flex items-center gap-2"
            >
              <Pencil className="w-4 h-4" /> Edit
            </button>
            <button
              onClick={() => {
                onDelete(task);
                setShowMenu(false);
              }}
              className="w-full px-4 py-2 text-left text-sm hover:bg-gray-100 text-red-600 flex items-center gap-2"
            >
              <Trash2 className="w-4 h-4" /> Delete
            </button>
          </div>
        )}
      </div>
    </div>
  );
}

// ============================================
// Task Modal
// ============================================

interface TaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateTaskRequest) => Promise<void>;
  task?: Task | null;
  projectId: string;
}

function TaskModal({ isOpen, onClose, onSave, task, projectId }: TaskModalProps) {
  const [formData, setFormData] = useState<CreateTaskRequest>({
    title: "",
    description: "",
    status: 0,
    priority: 1,
    projectId,
    dueDate: "",
  });
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (task) {
      setFormData({
        title: task.title,
        description: task.description || "",
        status: task.status,
        priority: task.priority,
        projectId,
        assigneeId: task.assigneeId,
        dueDate: task.dueDate?.split("T")[0] || "",
      });
    } else {
      setFormData({
        title: "",
        description: "",
        status: 0,
        priority: 1,
        projectId,
        dueDate: "",
      });
    }
  }, [task, isOpen, projectId]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      await onSave(formData);
      onClose();
    } catch (error) {
      console.error("Failed to save task:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={task ? "Edit Task" : "Create New Task"}
      size="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Task Title"
          value={formData.title}
          onChange={(e) => setFormData({ ...formData, title: e.target.value })}
          placeholder="Enter task title"
          required
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1.5">
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            placeholder="Enter task description"
            rows={3}
            className="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>

        <div className="grid grid-cols-2 gap-4">
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Status
            </label>
            <select
              value={formData.status}
              onChange={(e) => setFormData({ ...formData, status: Number(e.target.value) })}
              className="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value={0}>To Do</option>
              <option value={1}>In Progress</option>
              <option value={2}>In Review</option>
              <option value={3}>Done</option>
              <option value={4}>Cancelled</option>
            </select>
          </div>
          <div>
            <label className="block text-sm font-medium text-gray-700 mb-1.5">
              Priority
            </label>
            <select
              value={formData.priority}
              onChange={(e) => setFormData({ ...formData, priority: Number(e.target.value) })}
              className="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
            >
              <option value={0}>Low</option>
              <option value={1}>Medium</option>
              <option value={2}>High</option>
              <option value={3}>Critical</option>
            </select>
          </div>
        </div>

        <Input
          label="Due Date"
          type="date"
          value={formData.dueDate}
          onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
        />

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isLoading}>
            {task ? "Save Changes" : "Create Task"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

// ============================================
// Delete Task Modal
// ============================================

interface DeleteTaskModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
  taskTitle: string;
}

function DeleteTaskModal({ isOpen, onClose, onConfirm, taskTitle }: DeleteTaskModalProps) {
  const [isLoading, setIsLoading] = useState(false);

  const handleDelete = async () => {
    setIsLoading(true);
    try {
      await onConfirm();
      onClose();
    } catch (error) {
      console.error("Failed to delete task:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Delete Task" size="sm">
      <div className="space-y-4">
        <p className="text-gray-600">
          Are you sure you want to delete <strong>{taskTitle}</strong>?
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleDelete} isLoading={isLoading}>
            Delete Task
          </Button>
        </div>
      </div>
    </Modal>
  );
}

// ============================================
// Main Project Detail Page
// ============================================

export default function ProjectDetailPage() {
  const { projectId: id } = useParams<{ projectId: string }>();
  const navigate = useNavigate();
  const [project, setProject] = useState<Project | null>(null);
  const [tasks, setTasks] = useState<Task[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isTaskModalOpen, setIsTaskModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [taskToDelete, setTaskToDelete] = useState<Task | null>(null);

  useEffect(() => {
    if (id) {
      fetchProjectData();
    }
  }, [id]);

  const fetchProjectData = async () => {
    if (!id) return;
    try {
      setIsLoading(true);
      const [projectData, tasksData] = await Promise.all([
        projectService.getProjectById(id),
        taskService.getTasksByProjectId(id),
      ]);
      setProject(projectData);
      setTasks(tasksData);
    } catch (error) {
      console.error("Failed to fetch project data:", error);
      toast.error("Failed to load project");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateTask = async (data: CreateTaskRequest) => {
    const newTask = await taskService.createTask(data);
    setTasks([...tasks, newTask]);
    toast.success("Task created successfully!");
  };

  const handleUpdateTask = async (data: CreateTaskRequest) => {
    if (!selectedTask) return;
    const updatedTask = await taskService.updateTask(selectedTask.id, data);
    setTasks(tasks.map((t) => (t.id === updatedTask.id ? updatedTask : t)));
    toast.success("Task updated successfully!");
  };

  const handleDeleteTask = async () => {
    if (!taskToDelete) return;
    await taskService.deleteTask(taskToDelete.id);
    setTasks(tasks.filter((t) => t.id !== taskToDelete.id));
    toast.success("Task deleted successfully!");
  };

  const handleStatusChange = async (task: Task, newStatus: TaskStatus) => {
    try {
      const updatedTask = await taskService.updateTaskStatus(task.id, newStatus);
      setTasks(tasks.map((t) => (t.id === updatedTask.id ? updatedTask : t)));
      toast.success("Task status updated!");
    } catch (error) {
      console.error("Failed to update task status:", error);
      toast.error("Failed to update status");
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  if (!project) {
    return (
      <div className="text-center py-12">
        <h2 className="text-xl font-semibold text-gray-900">Project not found</h2>
        <Button onClick={() => navigate("/projects")} className="mt-4">
          Back to Projects
        </Button>
      </div>
    );
  }

  const completedTasks = tasks.filter((t) => t.status === 3).length;
  const progress = tasks.length > 0 ? Math.round((completedTasks / tasks.length) * 100) : 0;

  return (
    <div>
      {/* Back Button */}
      <button
        onClick={() => navigate("/projects")}
        className="flex items-center gap-2 text-gray-600 hover:text-gray-900 mb-6"
      >
        <ArrowLeft className="w-5 h-5" />
        Back to Projects
      </button>

      {/* Project Header */}
      <div className="flex items-start justify-between mb-8">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">{project.name}</h1>
          {project.description && (
            <p className="text-gray-600 mt-2 max-w-2xl">{project.description}</p>
          )}
        </div>
        <Button onClick={() => setIsTaskModalOpen(true)} icon={<Plus className="w-5 h-5" />}>
          Add Task
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4 mb-8">
        <Card padding="sm">
          <div className="text-sm text-gray-500">Total Tasks</div>
          <div className="text-2xl font-bold text-gray-900">{tasks.length}</div>
        </Card>
        <Card padding="sm">
          <div className="text-sm text-gray-500">Completed</div>
          <div className="text-2xl font-bold text-green-600">{completedTasks}</div>
        </Card>
        <Card padding="sm">
          <div className="text-sm text-gray-500">In Progress</div>
          <div className="text-2xl font-bold text-blue-600">
            {tasks.filter((t) => t.status === 1 || t.status === 2).length}
          </div>
        </Card>
        <Card padding="sm">
          <div className="text-sm text-gray-500">Progress</div>
          <div className="text-2xl font-bold text-primary-600">{progress}%</div>
        </Card>
      </div>

      {/* Progress Bar */}
      <div className="mb-8">
        <div className="h-2 bg-gray-200 rounded-full overflow-hidden">
          <div
            className="h-full bg-primary-600 transition-all duration-300"
            style={{ width: `${progress}%` }}
          />
        </div>
      </div>

      {/* Tasks List */}
      <Card title="Tasks" padding="none">
        {tasks.length === 0 ? (
          <div className="p-8 text-center">
            <p className="text-gray-500 mb-4">No tasks yet. Create your first task!</p>
            <Button onClick={() => setIsTaskModalOpen(true)} icon={<Plus className="w-5 h-5" />}>
              Create Task
            </Button>
          </div>
        ) : (
          <div className="divide-y">
            {tasks.map((task) => (
              <TaskItem
                key={task.id}
                task={task}
                onEdit={(t) => {
                  setSelectedTask(t);
                  setIsTaskModalOpen(true);
                }}
                onDelete={(t) => {
                  setTaskToDelete(t);
                  setIsDeleteModalOpen(true);
                }}
                onStatusChange={handleStatusChange}
              />
            ))}
          </div>
        )}
      </Card>

      {/* Task Modal */}
      <TaskModal
        isOpen={isTaskModalOpen}
        onClose={() => {
          setIsTaskModalOpen(false);
          setSelectedTask(null);
        }}
        onSave={selectedTask ? handleUpdateTask : handleCreateTask}
        task={selectedTask}
        projectId={id!}
      />

      {/* Delete Task Modal */}
      <DeleteTaskModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setTaskToDelete(null);
        }}
        onConfirm={handleDeleteTask}
        taskTitle={taskToDelete?.title || ""}
      />
    </div>
  );
}
