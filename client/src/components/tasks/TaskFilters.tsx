// ============================================
// Task Filters Component
// ============================================
// Combined search and filter controls for task lists

import { Search, X } from "lucide-react";
import Input from "../ui/Input";
import Select from "../ui/Select";
import Button from "../ui/Button";
import { TaskStatus, TaskPriority } from "../../types";

// ============================================
// Task Filters Props
// ============================================

interface TaskFiltersProps {
  searchTerm: string;
  onSearchChange: (value: string) => void;
  status: TaskStatus | undefined;
  onStatusChange: (value: TaskStatus | undefined) => void;
  priority: TaskPriority | undefined;
  onPriorityChange: (value: TaskPriority | undefined) => void;
  onClearFilters?: () => void;
}

// ============================================
// Filter Options
// ============================================

const statusOptions = [
  { value: TaskStatus.Todo, label: "To Do" },
  { value: TaskStatus.InProgress, label: "In Progress" },
  { value: TaskStatus.InReview, label: "In Review" },
  { value: TaskStatus.Done, label: "Done" },
  { value: TaskStatus.Cancelled, label: "Cancelled" },
];

const priorityOptions = [
  { value: TaskPriority.Low, label: "Low" },
  { value: TaskPriority.Medium, label: "Medium" },
  { value: TaskPriority.High, label: "High" },
  { value: TaskPriority.Critical, label: "Critical" },
];

// ============================================
// Task Filters Component
// ============================================

export default function TaskFilters({
  searchTerm,
  onSearchChange,
  status,
  onStatusChange,
  priority,
  onPriorityChange,
  onClearFilters,
}: TaskFiltersProps) {
  const hasFilters = searchTerm || status !== undefined || priority !== undefined;

  const handleStatusChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    onStatusChange(value === "" ? undefined : Number(value) as TaskStatus);
  };

  const handlePriorityChange = (e: React.ChangeEvent<HTMLSelectElement>) => {
    const value = e.target.value;
    onPriorityChange(value === "" ? undefined : Number(value) as TaskPriority);
  };

  return (
    <div className="flex flex-wrap items-end gap-4">
      {/* Search Input */}
      <div className="flex-1 min-w-[200px] max-w-md">
        <Input
          label="Search"
          placeholder="Search tasks..."
          value={searchTerm}
          onChange={(e) => onSearchChange(e.target.value)}
          icon={<Search className="w-5 h-5" />}
        />
      </div>

      {/* Status Filter */}
      <div className="w-40">
        <Select
          label="Status"
          placeholder="All Statuses"
          options={statusOptions}
          value={status ?? ""}
          onChange={handleStatusChange}
        />
      </div>

      {/* Priority Filter */}
      <div className="w-40">
        <Select
          label="Priority"
          placeholder="All Priorities"
          options={priorityOptions}
          value={priority ?? ""}
          onChange={handlePriorityChange}
        />
      </div>

      {/* Clear Filters Button */}
      {hasFilters && onClearFilters && (
        <Button
          variant="ghost"
          size="sm"
          onClick={onClearFilters}
          className="flex items-center gap-1"
        >
          <X className="w-4 h-4" />
          Clear
        </Button>
      )}
    </div>
  );
}
