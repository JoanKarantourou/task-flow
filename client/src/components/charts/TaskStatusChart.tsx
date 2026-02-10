// ============================================
// Task Status Chart Component
// ============================================
// Pie/Donut chart showing task distribution by status

import { PieChart, Pie, Cell, ResponsiveContainer, Legend, Tooltip } from "recharts";

// ============================================
// Props
// ============================================

interface TaskStatusChartProps {
  data: Record<string, number>;
}

// ============================================
// Status Colors
// ============================================

const STATUS_COLORS: Record<string, string> = {
  Todo: "#9CA3AF",      // gray-400
  InProgress: "#3B82F6", // blue-500
  InReview: "#F59E0B",   // amber-500
  Done: "#10B981",       // emerald-500
  Cancelled: "#EF4444",  // red-500
};

const STATUS_LABELS: Record<string, string> = {
  Todo: "To Do",
  InProgress: "In Progress",
  InReview: "In Review",
  Done: "Done",
  Cancelled: "Cancelled",
};

// ============================================
// Task Status Chart Component
// ============================================

export default function TaskStatusChart({ data }: TaskStatusChartProps) {
  // Transform data for Recharts
  const chartData = Object.entries(data)
    .filter(([_, value]) => value > 0)
    .map(([status, value]) => ({
      name: STATUS_LABELS[status] || status,
      value,
      color: STATUS_COLORS[status] || "#6B7280",
    }));

  if (chartData.length === 0) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500">
        No tasks to display
      </div>
    );
  }

  return (
    <div className="h-64">
      <ResponsiveContainer width="100%" height="100%">
        <PieChart>
          <Pie
            data={chartData}
            cx="50%"
            cy="50%"
            innerRadius={50}
            outerRadius={80}
            paddingAngle={2}
            dataKey="value"
            label={({ name, percent }) =>
              `${name} (${((percent ?? 0) * 100).toFixed(0)}%)`
            }
            labelLine={false}
          >
            {chartData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Pie>
          <Tooltip
            formatter={(value) => [value, "Tasks"]}
            contentStyle={{
              backgroundColor: "white",
              border: "1px solid #E5E7EB",
              borderRadius: "8px",
              padding: "8px 12px",
            }}
          />
          <Legend
            verticalAlign="bottom"
            height={36}
            formatter={(value) => (
              <span className="text-sm text-gray-700">{value}</span>
            )}
          />
        </PieChart>
      </ResponsiveContainer>
    </div>
  );
}
