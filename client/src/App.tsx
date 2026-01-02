import { useAuth } from "./context/AuthContext";
import MainLayout from "./components/layout/MainLayout";
import Card from "./components/ui/Card";
import Badge from "./components/ui/Badge";
import Button from "./components/ui/Button";

function App() {
  const { user, isAuthenticated } = useAuth();

  // If not authenticated, show a simple message
  if (!isAuthenticated) {
    return (
      <div className="min-h-screen bg-gray-100 flex items-center justify-center">
        <Card className="max-w-md">
          <h1 className="text-2xl font-bold text-gray-900 mb-4">
            Welcome to TaskFlow
          </h1>
          <p className="text-gray-600 mb-4">
            You're not logged in yet. The login page will come next!
          </p>
          <Badge variant="info">Layout is ready! ✅</Badge>
        </Card>
      </div>
    );
  }

  // If authenticated, show the main layout
  return (
    <MainLayout>
      {/* Dashboard Content Example */}
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
                <p className="text-3xl font-bold text-gray-900 mt-1">12</p>
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
                <p className="text-3xl font-bold text-gray-900 mt-1">28</p>
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
                <p className="text-3xl font-bold text-gray-900 mt-1">156</p>
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
          <div className="space-y-3">
            {/* Task Item */}
            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div className="flex items-center gap-3">
                <input type="checkbox" className="w-4 h-4 rounded" />
                <span className="text-gray-900">Update documentation</span>
              </div>
              <Badge variant="warning">In Progress</Badge>
            </div>

            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div className="flex items-center gap-3">
                <input type="checkbox" className="w-4 h-4 rounded" />
                <span className="text-gray-900">Review pull requests</span>
              </div>
              <Badge variant="info">Todo</Badge>
            </div>

            <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
              <div className="flex items-center gap-3">
                <input type="checkbox" className="w-4 h-4 rounded" checked />
                <span className="text-gray-900 line-through">
                  Fix bug in login
                </span>
              </div>
              <Badge variant="success">Done</Badge>
            </div>
          </div>

          <div className="mt-4">
            <Button variant="ghost" fullWidth>
              View All Tasks →
            </Button>
          </div>
        </Card>
      </div>
    </MainLayout>
  );
}

export default App;
