// ============================================
// Task Priority Chart Component
// ============================================
// Bar chart showing tasks by priority level

import { BarChart, Bar, XAxis, YAxis, CartesianGrid, Tooltip, ResponsiveContainer, Cell } from "recharts";

// ============================================
// Props
// ============================================

interface TaskPriorityChartProps {
  data: Record<string, number>;
}

// ============================================
// Priority Colors
// ============================================

const PRIORITY_COLORS: Record<string, string> = {
  Low: "#10B981",       // emerald-500
  Medium: "#3B82F6",    // blue-500
  High: "#F59E0B",      // amber-500
  Critical: "#EF4444",  // red-500
};

const PRIORITY_ORDER = ["Low", "Medium", "High", "Critical"];

// ============================================
// Task Priority Chart Component
// ============================================

export default function TaskPriorityChart({ data }: TaskPriorityChartProps) {
  // Transform and order data for Recharts
  const chartData = PRIORITY_ORDER.map((priority) => ({
    name: priority,
    value: data[priority] || 0,
    color: PRIORITY_COLORS[priority],
  }));

  const hasData = chartData.some((item) => item.value > 0);

  if (!hasData) {
    return (
      <div className="flex items-center justify-center h-64 text-gray-500">
        No tasks to display
      </div>
    );
  }

  return (
    <div className="h-64">
      <ResponsiveContainer width="100%" height="100%">
        <BarChart
          data={chartData}
          layout="vertical"
          margin={{ top: 5, right: 30, left: 60, bottom: 5 }}
        >
          <CartesianGrid strokeDasharray="3 3" horizontal={true} vertical={false} />
          <XAxis type="number" allowDecimals={false} />
          <YAxis
            type="category"
            dataKey="name"
            tick={{ fontSize: 12 }}
            width={60}
          />
          <Tooltip
            formatter={(value) => [value, "Tasks"]}
            contentStyle={{
              backgroundColor: "white",
              border: "1px solid #E5E7EB",
              borderRadius: "8px",
              padding: "8px 12px",
            }}
          />
          <Bar dataKey="value" radius={[0, 4, 4, 0]}>
            {chartData.map((entry, index) => (
              <Cell key={`cell-${index}`} fill={entry.color} />
            ))}
          </Bar>
        </BarChart>
      </ResponsiveContainer>
    </div>
  );
}
