// ============================================
// Login Page
// ============================================
// Authentication page with login and register tabs
// Uses React Hook Form + Zod for validation

import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { useForm } from "react-hook-form";
import { zodResolver } from "@hookform/resolvers/zod";
import { z } from "zod";
import toast from "react-hot-toast";
import { useAuth } from "../context/AuthContext";
import Button from "../components/ui/Button";
import Input from "../components/ui/Input";
import Card from "../components/ui/Card";

// ============================================
// Validation Schemas
// ============================================

const loginSchema = z.object({
  email: z.string().email("Invalid email address"),
  password: z.string().min(6, "Password must be at least 6 characters"),
});

const registerSchema = z
  .object({
    firstName: z.string().min(1, "First name is required"),
    lastName: z.string().min(1, "Last name is required"),
    email: z.string().email("Invalid email address"),
    password: z.string().min(6, "Password must be at least 6 characters"),
    confirmPassword: z.string(),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: "Passwords don't match",
    path: ["confirmPassword"],
  });

type LoginFormData = z.infer<typeof loginSchema>;
type RegisterFormData = z.infer<typeof registerSchema>;

// ============================================
// Login Page Component
// ============================================

export default function LoginPage() {
  const [activeTab, setActiveTab] = useState<"login" | "register">("login");
  const navigate = useNavigate();
  const { login, register: registerUser } = useAuth();

  // Login form
  const {
    register: registerLogin,
    handleSubmit: handleLoginSubmit,
    formState: { errors: loginErrors, isSubmitting: isLoginSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
  });

  // Register form
  const {
    register: registerRegister,
    handleSubmit: handleRegisterSubmit,
    formState: { errors: registerErrors, isSubmitting: isRegisterSubmitting },
  } = useForm<RegisterFormData>({
    resolver: zodResolver(registerSchema),
  });

  // Handle login submission
  async function onLoginSubmit(data: LoginFormData) {
    try {
      await login(data);
      toast.success("Login successful!");
      navigate("/dashboard");
    } catch (error) {
      toast.error(error instanceof Error ? error.message : "Login failed");
    }
  }

  // Handle register submission
  async function onRegisterSubmit(data: RegisterFormData) {
    try {
      await registerUser({
        email: data.email,
        password: data.password,
        firstName: data.firstName,
        lastName: data.lastName,
      });
      toast.success("Registration successful!");
      navigate("/dashboard");
    } catch (error) {
      toast.error(
        error instanceof Error ? error.message : "Registration failed"
      );
    }
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 flex items-center justify-center p-4">
      <div className="w-full max-w-md">
        {/* Logo/Header */}
        <div className="text-center mb-8">
          <h1 className="text-4xl font-bold text-gray-900 mb-2">TaskFlow</h1>
          <p className="text-gray-600">
            Manage your projects and tasks efficiently
          </p>
        </div>

        <Card className="shadow-xl">
          {/* Tabs */}
          <div className="flex border-b border-gray-200 mb-6">
            <button
              onClick={() => setActiveTab("login")}
              className={`flex-1 py-2 text-center font-medium border-b-2 transition-colors ${
                activeTab === "login"
                  ? "border-blue-600 text-blue-600"
                  : "border-transparent text-gray-600 hover:text-gray-900"
              }`}
            >
              Login
            </button>
            <button
              onClick={() => setActiveTab("register")}
              className={`flex-1 py-2 text-center font-medium border-b-2 transition-colors ${
                activeTab === "register"
                  ? "border-blue-600 text-blue-600"
                  : "border-transparent text-gray-600 hover:text-gray-900"
              }`}
            >
              Register
            </button>
          </div>

          {/* Login Form */}
          {activeTab === "login" && (
            <form onSubmit={handleLoginSubmit(onLoginSubmit)} className="space-y-4">
              <Input
                label="Email"
                type="email"
                placeholder="you@example.com"
                error={loginErrors.email?.message}
                {...registerLogin("email")}
              />

              <Input
                label="Password"
                type="password"
                placeholder="Enter your password"
                error={loginErrors.password?.message}
                {...registerLogin("password")}
              />

              <div className="flex items-center justify-between">
                <label className="flex items-center">
                  <input
                    type="checkbox"
                    className="w-4 h-4 rounded border-gray-300 text-blue-600 focus:ring-blue-500"
                  />
                  <span className="ml-2 text-sm text-gray-600">
                    Remember me
                  </span>
                </label>
                <a href="#" className="text-sm text-blue-600 hover:text-blue-700">
                  Forgot password?
                </a>
              </div>

              <Button
                type="submit"
                variant="primary"
                fullWidth
                isLoading={isLoginSubmitting}
              >
                {isLoginSubmitting ? "Logging in..." : "Login"}
              </Button>
            </form>
          )}

          {/* Register Form */}
          {activeTab === "register" && (
            <form
              onSubmit={handleRegisterSubmit(onRegisterSubmit)}
              className="space-y-4"
            >
              <div className="grid grid-cols-2 gap-4">
                <Input
                  label="First Name"
                  type="text"
                  placeholder="John"
                  error={registerErrors.firstName?.message}
                  {...registerRegister("firstName")}
                />

                <Input
                  label="Last Name"
                  type="text"
                  placeholder="Doe"
                  error={registerErrors.lastName?.message}
                  {...registerRegister("lastName")}
                />
              </div>

              <Input
                label="Email"
                type="email"
                placeholder="you@example.com"
                error={registerErrors.email?.message}
                {...registerRegister("email")}
              />

              <Input
                label="Password"
                type="password"
                placeholder="At least 6 characters"
                error={registerErrors.password?.message}
                {...registerRegister("password")}
              />

              <Input
                label="Confirm Password"
                type="password"
                placeholder="Re-enter your password"
                error={registerErrors.confirmPassword?.message}
                {...registerRegister("confirmPassword")}
              />

              <Button
                type="submit"
                variant="primary"
                fullWidth
                isLoading={isRegisterSubmitting}
              >
                {isRegisterSubmitting ? "Creating account..." : "Create Account"}
              </Button>
            </form>
          )}
        </Card>

        {/* Footer */}
        <p className="text-center text-sm text-gray-600 mt-6">
          By continuing, you agree to our Terms of Service and Privacy Policy
        </p>
      </div>
    </div>
  );
}
