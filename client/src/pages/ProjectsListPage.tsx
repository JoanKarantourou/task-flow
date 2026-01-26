// ============================================
// Projects List Page
// ============================================
// Display all projects in grid view with create/edit/delete functionality

import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Plus, FolderOpen, Calendar, Users, MoreVertical, Pencil, Trash2 } from "lucide-react";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import Modal from "../components/ui/Modal";
import Input from "../components/ui/Input";
import Badge from "../components/ui/Badge";
import projectService from "../services/projectService";
import type { Project, CreateProjectRequest, ProjectStatus } from "../types";
import toast from "react-hot-toast";

// ============================================
// Helper Functions
// ============================================

const getStatusBadge = (status: ProjectStatus) => {
  const statusConfig = {
    0: { label: "Active", variant: "success" as const },
    1: { label: "On Hold", variant: "warning" as const },
    2: { label: "Completed", variant: "info" as const },
    3: { label: "Archived", variant: "neutral" as const },
  };
  const config = statusConfig[status] || statusConfig[0];
  return <Badge variant={config.variant}>{config.label}</Badge>;
};

const formatDate = (dateString?: string) => {
  if (!dateString) return "No date set";
  return new Date(dateString).toLocaleDateString("en-US", {
    month: "short",
    day: "numeric",
    year: "numeric",
  });
};

// ============================================
// Project Card Component
// ============================================

interface ProjectCardProps {
  project: Project;
  onEdit: (project: Project) => void;
  onDelete: (project: Project) => void;
  onClick: (project: Project) => void;
}

function ProjectCard({ project, onEdit, onDelete, onClick }: ProjectCardProps) {
  const [showMenu, setShowMenu] = useState(false);

  return (
    <Card
      hoverable
      className="relative"
      onClick={() => onClick(project)}
    >
      {/* Menu Button */}
      <div className="absolute top-4 right-4">
        <button
          onClick={(e) => {
            e.stopPropagation();
            setShowMenu(!showMenu);
          }}
          className="p-1 rounded hover:bg-gray-100"
        >
          <MoreVertical className="w-5 h-5 text-gray-500" />
        </button>

        {/* Dropdown Menu */}
        {showMenu && (
          <div className="absolute right-0 mt-1 w-36 bg-white rounded-lg shadow-lg border z-10">
            <button
              onClick={(e) => {
                e.stopPropagation();
                onEdit(project);
                setShowMenu(false);
              }}
              className="w-full px-4 py-2 text-left text-sm hover:bg-gray-100 flex items-center gap-2"
            >
              <Pencil className="w-4 h-4" /> Edit
            </button>
            <button
              onClick={(e) => {
                e.stopPropagation();
                onDelete(project);
                setShowMenu(false);
              }}
              className="w-full px-4 py-2 text-left text-sm hover:bg-gray-100 text-red-600 flex items-center gap-2"
            >
              <Trash2 className="w-4 h-4" /> Delete
            </button>
          </div>
        )}
      </div>

      {/* Project Icon */}
      <div className="w-12 h-12 bg-primary-100 rounded-lg flex items-center justify-center mb-4">
        <FolderOpen className="w-6 h-6 text-primary-600" />
      </div>

      {/* Project Info */}
      <h3 className="text-lg font-semibold text-gray-900 mb-2 pr-8">
        {project.name}
      </h3>

      <p className="text-gray-600 text-sm mb-4 line-clamp-2">
        {project.description || "No description provided"}
      </p>

      {/* Status Badge */}
      <div className="mb-4">
        {getStatusBadge(project.status)}
      </div>

      {/* Meta Info */}
      <div className="flex items-center gap-4 text-sm text-gray-500">
        <div className="flex items-center gap-1">
          <Calendar className="w-4 h-4" />
          <span>{formatDate(project.dueDate)}</span>
        </div>
        {project.memberCount !== undefined && (
          <div className="flex items-center gap-1">
            <Users className="w-4 h-4" />
            <span>{project.memberCount}</span>
          </div>
        )}
      </div>
    </Card>
  );
}

// ============================================
// Create/Edit Project Modal
// ============================================

interface ProjectModalProps {
  isOpen: boolean;
  onClose: () => void;
  onSave: (data: CreateProjectRequest) => Promise<void>;
  project?: Project | null;
}

function ProjectModal({ isOpen, onClose, onSave, project }: ProjectModalProps) {
  const [formData, setFormData] = useState<CreateProjectRequest>({
    name: "",
    description: "",
    status: 0,
    startDate: "",
    dueDate: "",
  });
  const [isLoading, setIsLoading] = useState(false);

  useEffect(() => {
    if (project) {
      setFormData({
        name: project.name,
        description: project.description || "",
        status: project.status,
        startDate: project.startDate?.split("T")[0] || "",
        dueDate: project.dueDate?.split("T")[0] || "",
      });
    } else {
      setFormData({
        name: "",
        description: "",
        status: 0,
        startDate: "",
        dueDate: "",
      });
    }
  }, [project, isOpen]);

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setIsLoading(true);
    try {
      await onSave(formData);
      onClose();
    } catch (error) {
      console.error("Failed to save project:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal
      isOpen={isOpen}
      onClose={onClose}
      title={project ? "Edit Project" : "Create New Project"}
      size="md"
    >
      <form onSubmit={handleSubmit} className="space-y-4">
        <Input
          label="Project Name"
          value={formData.name}
          onChange={(e) => setFormData({ ...formData, name: e.target.value })}
          placeholder="Enter project name"
          required
        />

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1.5">
            Description
          </label>
          <textarea
            value={formData.description}
            onChange={(e) => setFormData({ ...formData, description: e.target.value })}
            placeholder="Enter project description"
            rows={3}
            className="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-gray-700 mb-1.5">
            Status
          </label>
          <select
            value={formData.status}
            onChange={(e) => setFormData({ ...formData, status: Number(e.target.value) })}
            className="block w-full rounded-lg border border-gray-300 px-4 py-2.5 text-gray-900 focus:outline-none focus:ring-2 focus:ring-primary-500 focus:border-primary-500"
          >
            <option value={0}>Active</option>
            <option value={1}>On Hold</option>
            <option value={2}>Completed</option>
            <option value={3}>Archived</option>
          </select>
        </div>

        <div className="grid grid-cols-2 gap-4">
          <Input
            label="Start Date"
            type="date"
            value={formData.startDate}
            onChange={(e) => setFormData({ ...formData, startDate: e.target.value })}
          />
          <Input
            label="Due Date"
            type="date"
            value={formData.dueDate}
            onChange={(e) => setFormData({ ...formData, dueDate: e.target.value })}
          />
        </div>

        <div className="flex justify-end gap-3 pt-4">
          <Button type="button" variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button type="submit" isLoading={isLoading}>
            {project ? "Save Changes" : "Create Project"}
          </Button>
        </div>
      </form>
    </Modal>
  );
}

// ============================================
// Delete Confirmation Modal
// ============================================

interface DeleteModalProps {
  isOpen: boolean;
  onClose: () => void;
  onConfirm: () => Promise<void>;
  projectName: string;
}

function DeleteModal({ isOpen, onClose, onConfirm, projectName }: DeleteModalProps) {
  const [isLoading, setIsLoading] = useState(false);

  const handleDelete = async () => {
    setIsLoading(true);
    try {
      await onConfirm();
      onClose();
    } catch (error) {
      console.error("Failed to delete project:", error);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <Modal isOpen={isOpen} onClose={onClose} title="Delete Project" size="sm">
      <div className="space-y-4">
        <p className="text-gray-600">
          Are you sure you want to delete <strong>{projectName}</strong>? This action cannot be undone and will delete all associated tasks.
        </p>
        <div className="flex justify-end gap-3">
          <Button variant="secondary" onClick={onClose}>
            Cancel
          </Button>
          <Button variant="danger" onClick={handleDelete} isLoading={isLoading}>
            Delete Project
          </Button>
        </div>
      </div>
    </Modal>
  );
}

// ============================================
// Main Projects List Page
// ============================================

export default function ProjectsListPage() {
  const navigate = useNavigate();
  const [projects, setProjects] = useState<Project[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [isDeleteModalOpen, setIsDeleteModalOpen] = useState(false);
  const [selectedProject, setSelectedProject] = useState<Project | null>(null);
  const [projectToDelete, setProjectToDelete] = useState<Project | null>(null);

  // Fetch projects on mount
  useEffect(() => {
    fetchProjects();
  }, []);

  const fetchProjects = async () => {
    try {
      setIsLoading(true);
      const data = await projectService.getMyProjects();
      setProjects(data);
    } catch (error) {
      console.error("Failed to fetch projects:", error);
      toast.error("Failed to load projects");
    } finally {
      setIsLoading(false);
    }
  };

  const handleCreateProject = async (data: CreateProjectRequest) => {
    const newProject = await projectService.createProject(data);
    setProjects([newProject, ...projects]);
    toast.success("Project created successfully!");
  };

  const handleUpdateProject = async (data: CreateProjectRequest) => {
    if (!selectedProject) return;
    const updatedProject = await projectService.updateProject(selectedProject.id, data);
    setProjects(projects.map((p) => (p.id === updatedProject.id ? updatedProject : p)));
    toast.success("Project updated successfully!");
  };

  const handleDeleteProject = async () => {
    if (!projectToDelete) return;
    await projectService.deleteProject(projectToDelete.id);
    setProjects(projects.filter((p) => p.id !== projectToDelete.id));
    toast.success("Project deleted successfully!");
  };

  const openEditModal = (project: Project) => {
    setSelectedProject(project);
    setIsModalOpen(true);
  };

  const openDeleteModal = (project: Project) => {
    setProjectToDelete(project);
    setIsDeleteModalOpen(true);
  };

  const openCreateModal = () => {
    setSelectedProject(null);
    setIsModalOpen(true);
  };

  const handleProjectClick = (project: Project) => {
    navigate(`/projects/${project.id}`);
  };

  // Loading state
  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div>
      {/* Header */}
      <div className="flex items-center justify-between mb-6">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Projects</h1>
          <p className="text-gray-600 mt-1">Manage your projects and track progress</p>
        </div>
        <Button onClick={openCreateModal} icon={<Plus className="w-5 h-5" />}>
          New Project
        </Button>
      </div>

      {/* Projects Grid */}
      {projects.length === 0 ? (
        <Card className="text-center py-12">
          <FolderOpen className="w-16 h-16 text-gray-300 mx-auto mb-4" />
          <h3 className="text-lg font-medium text-gray-900 mb-2">No projects yet</h3>
          <p className="text-gray-600 mb-4">Get started by creating your first project</p>
          <Button onClick={openCreateModal} icon={<Plus className="w-5 h-5" />}>
            Create Project
          </Button>
        </Card>
      ) : (
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {projects.map((project) => (
            <ProjectCard
              key={project.id}
              project={project}
              onEdit={openEditModal}
              onDelete={openDeleteModal}
              onClick={handleProjectClick}
            />
          ))}
        </div>
      )}

      {/* Create/Edit Modal */}
      <ProjectModal
        isOpen={isModalOpen}
        onClose={() => {
          setIsModalOpen(false);
          setSelectedProject(null);
        }}
        onSave={selectedProject ? handleUpdateProject : handleCreateProject}
        project={selectedProject}
      />

      {/* Delete Confirmation Modal */}
      <DeleteModal
        isOpen={isDeleteModalOpen}
        onClose={() => {
          setIsDeleteModalOpen(false);
          setProjectToDelete(null);
        }}
        onConfirm={handleDeleteProject}
        projectName={projectToDelete?.name || ""}
      />
    </div>
  );
}
