// ============================================
// Profile Page
// ============================================
// User profile management and settings

import { useState, useEffect } from "react";
import { User, Mail, Save, LogOut } from "lucide-react";
import { useNavigate } from "react-router-dom";
import Card from "../components/ui/Card";
import Button from "../components/ui/Button";
import Input from "../components/ui/Input";
import { useAuth } from "../context/AuthContext";
import apiClient from "../services/api";
import toast from "react-hot-toast";

// ============================================
// Profile Form Data
// ============================================

interface ProfileFormData {
  firstName: string;
  lastName: string;
  email: string;
}

// ============================================
// Main Profile Page
// ============================================

export default function ProfilePage() {
  const navigate = useNavigate();
  const { user, logout, updateUser } = useAuth();
  const [formData, setFormData] = useState<ProfileFormData>({
    firstName: "",
    lastName: "",
    email: "",
  });
  const [isLoading, setIsLoading] = useState(false);
  const [hasChanges, setHasChanges] = useState(false);

  // Initialize form with user data
  useEffect(() => {
    if (user) {
      setFormData({
        firstName: user.firstName || "",
        lastName: user.lastName || "",
        email: user.email || "",
      });
    }
  }, [user]);

  // Check for changes
  useEffect(() => {
    if (user) {
      const changed =
        formData.firstName !== user.firstName ||
        formData.lastName !== user.lastName ||
        formData.email !== user.email;
      setHasChanges(changed);
    }
  }, [formData, user]);

  const handleInputChange = (field: keyof ProfileFormData, value: string) => {
    setFormData((prev) => ({ ...prev, [field]: value }));
  };

  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();

    if (!hasChanges) {
      toast.error("No changes to save");
      return;
    }

    setIsLoading(true);
    try {
      // Call API to update profile
      const response = await apiClient.put("/users/me", {
        firstName: formData.firstName,
        lastName: formData.lastName,
        email: formData.email,
      });

      // Update local user state
      updateUser(response.data);

      toast.success("Profile updated successfully!");
      setHasChanges(false);
    } catch (error: any) {
      console.error("Failed to update profile:", error);
      const message = error.response?.data?.message || "Failed to update profile";
      toast.error(message);
    } finally {
      setIsLoading(false);
    }
  };

  const handleLogout = () => {
    logout();
    navigate("/login");
    toast.success("Logged out successfully");
  };

  if (!user) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary-600"></div>
      </div>
    );
  }

  return (
    <div className="max-w-2xl mx-auto">
      {/* Header */}
      <div className="mb-8">
        <h1 className="text-3xl font-bold text-gray-900">Profile Settings</h1>
        <p className="text-gray-600 mt-1">Manage your account information</p>
      </div>

      {/* Profile Card */}
      <Card className="mb-6">
        {/* Avatar Section */}
        <div className="flex items-center gap-4 mb-8 pb-6 border-b">
          <div className="w-20 h-20 bg-primary-100 rounded-full flex items-center justify-center">
            <span className="text-3xl font-bold text-primary-600">
              {user.firstName?.charAt(0) || user.email.charAt(0).toUpperCase()}
            </span>
          </div>
          <div>
            <h2 className="text-xl font-semibold text-gray-900">
              {user.firstName} {user.lastName}
            </h2>
            <p className="text-gray-500">{user.email}</p>
          </div>
        </div>

        {/* Profile Form */}
        <form onSubmit={handleSubmit} className="space-y-6">
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            <Input
              label="First Name"
              value={formData.firstName}
              onChange={(e) => handleInputChange("firstName", e.target.value)}
              placeholder="Enter your first name"
              icon={<User className="w-5 h-5" />}
            />
            <Input
              label="Last Name"
              value={formData.lastName}
              onChange={(e) => handleInputChange("lastName", e.target.value)}
              placeholder="Enter your last name"
              icon={<User className="w-5 h-5" />}
            />
          </div>

          <Input
            label="Email Address"
            type="email"
            value={formData.email}
            onChange={(e) => handleInputChange("email", e.target.value)}
            placeholder="Enter your email"
            icon={<Mail className="w-5 h-5" />}
          />

          {/* Account Info */}
          <div className="pt-4 border-t">
            <h3 className="text-sm font-medium text-gray-700 mb-2">Account Information</h3>
            <div className="text-sm text-gray-500 space-y-1">
              <p>
                <span className="font-medium">Member since:</span>{" "}
                {new Date(user.createdAt).toLocaleDateString("en-US", {
                  year: "numeric",
                  month: "long",
                  day: "numeric",
                })}
              </p>
              <p>
                <span className="font-medium">User ID:</span>{" "}
                <span className="font-mono text-xs">{user.id}</span>
              </p>
            </div>
          </div>

          {/* Actions */}
          <div className="flex items-center justify-between pt-4">
            <Button
              type="button"
              variant="ghost"
              onClick={handleLogout}
              icon={<LogOut className="w-5 h-5" />}
              className="text-red-600 hover:bg-red-50"
            >
              Sign Out
            </Button>
            <Button
              type="submit"
              isLoading={isLoading}
              disabled={!hasChanges}
              icon={<Save className="w-5 h-5" />}
            >
              Save Changes
            </Button>
          </div>
        </form>
      </Card>

      {/* Danger Zone */}
      <Card>
        <h3 className="text-lg font-semibold text-red-600 mb-2">Danger Zone</h3>
        <p className="text-gray-600 text-sm mb-4">
          Once you delete your account, there is no going back. Please be certain.
        </p>
        <Button
          variant="danger"
          onClick={() => toast.error("Account deletion is not yet implemented")}
        >
          Delete Account
        </Button>
      </Card>
    </div>
  );
}
