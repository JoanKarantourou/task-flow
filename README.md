# TaskFlow

A full-stack task management platform built with .NET 8, React, PostgreSQL, RabbitMQ, and SignalR. Features real-time notifications, Kanban board, CQRS architecture, and JWT authentication.

## Tech Stack

| Layer | Technologies |
|-------|-------------|
| **Backend** | .NET 8, ASP.NET Core, Entity Framework Core, MediatR, FluentValidation, AutoMapper |
| **Frontend** | React 19, TypeScript, Vite, Tailwind CSS, React Router, React Query |
| **Database** | PostgreSQL 16, EF Core Migrations |
| **Messaging** | RabbitMQ, MassTransit |
| **Real-Time** | SignalR (WebSocket) |
| **DevOps** | Docker, Docker Compose |

## Architecture

The backend follows **Clean Architecture** with **CQRS** (Command Query Responsibility Segregation):

```
src/
  TaskFlow.Domain/          # Entities, enums, base classes (no dependencies)
  TaskFlow.Application/     # CQRS commands/queries, DTOs, interfaces, validators
  TaskFlow.Infrastructure/  # EF Core, RabbitMQ, JWT, SignalR implementations
  TaskFlow.API/             # Controllers, SignalR hubs, middleware
client/                     # React frontend
```

**Key patterns:** Generic Repository, Unit of Work, MediatR Pipeline Behaviors (validation, logging, performance monitoring), event-driven messaging via RabbitMQ consumers.

## Features

- **Authentication** -- JWT access tokens with refresh token rotation
- **Projects** -- Create, update, delete, and archive projects
- **Tasks** -- Full CRUD with status workflow (Todo, In Progress, In Review, Done, Cancelled) and priority levels
- **Kanban Board** -- Drag-and-drop tasks between status columns
- **Real-Time Notifications** -- SignalR pushes task/project updates to connected clients
- **Event-Driven Messaging** -- RabbitMQ publishes domain events (task created, assigned, status changed) consumed by background processors
- **Validation** -- FluentValidation on all commands with MediatR pipeline behavior
- **Global Error Handling** -- Centralized exception middleware with consistent JSON error responses

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 20+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/)

## Getting Started

### Option 1: Docker (recommended)

Run the entire stack with one command:

```bash
docker compose up --build
```

| Service | URL |
|---------|-----|
| Client | http://localhost:3000 |
| API / Swagger | http://localhost:5000 |
| RabbitMQ Management | http://localhost:15672 (guest / guest) |
| PostgreSQL | localhost:5433 (taskflow / taskflow123) |

### Option 2: Local Development

**1. Start infrastructure services:**

```bash
docker compose up postgres rabbitmq -d
```

**2. Run the API:**

```bash
cd src/TaskFlow.API
dotnet run
```

The API starts at `http://localhost:5154` (or the port in `launchSettings.json`).

**3. Run the client:**

```bash
cd client
npm install
npm run dev
```

The client starts at `http://localhost:5173`.

### Database Migrations

Migrations are applied automatically on startup. To create a new migration manually:

```bash
dotnet ef migrations add <MigrationName> \
  --project src/TaskFlow.Infrastructure \
  --startup-project src/TaskFlow.API
```

## Project Structure

```
task-flow/
  docker-compose.yml              # All services (postgres, rabbitmq, api, client)
  TaskFlow.sln                    # .NET solution
  src/
    TaskFlow.Domain/
      Entities/                   # User, Project, TaskItem, Comment, ProjectMember
      Enums/                      # TaskStatus, TaskPriority, ProjectStatus
      Common/                     # BaseEntity
    TaskFlow.Application/
      Features/                   # CQRS commands & queries per feature
        Auth/                     # Login, Register, RefreshToken
        Projects/                 # CRUD commands & queries
        Tasks/                    # CRUD + AssignTask, status changes
        Comments/                 # CRUD
        Users/                    # GetCurrentUser, UpdateProfile
      Common/
        Behaviors/                # ValidationBehavior, LoggingBehavior, PerformanceBehavior
        Mappings/                 # AutoMapper profiles
      Contracts/                  # RabbitMQ event contracts
      DTOs/                       # Data transfer objects
      Interfaces/                 # Repository, service, and UoW interfaces
    TaskFlow.Infrastructure/
      Persistence/                # DbContext, migrations, entity configurations
      Repositories/               # GenericRepository, UnitOfWork, specialized repos
      Identity/                   # AuthService, TokenService, CurrentUserService
      Messaging/                  # RabbitMQ consumers (MassTransit)
      Services/                   # SignalRNotificationService
    TaskFlow.API/
      Controllers/                # Auth, Projects, Tasks, Comments, Users
      Hubs/                       # NotificationHub (SignalR)
      Middleware/                  # ExceptionHandlingMiddleware
  client/
    src/
      components/                 # UI components (Button, Card, Modal, etc.) + layout
      context/                    # AuthContext, SignalRContext
      hooks/                      # useDebounce, useToast
      pages/                      # Dashboard, Projects, ProjectDetail, Tasks, Login, Profile
      services/                   # API client, auth, project, task, SignalR services
      types/                      # TypeScript interfaces matching backend DTOs
      utils/                      # Date, avatar, status color helpers
```

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/auth/register` | Register a new user |
| POST | `/api/auth/login` | Login and receive JWT tokens |
| POST | `/api/auth/refresh-token` | Refresh an expired access token |
| GET | `/api/projects` | Get all projects for current user |
| GET | `/api/projects/{id}` | Get project by ID |
| POST | `/api/projects` | Create a new project |
| PUT | `/api/projects/{id}` | Update a project |
| DELETE | `/api/projects/{id}` | Delete a project |
| GET | `/api/tasks/project/{projectId}` | Get tasks for a project |
| GET | `/api/tasks/{id}` | Get task by ID |
| POST | `/api/tasks` | Create a new task |
| PUT | `/api/tasks/{id}` | Update a task |
| DELETE | `/api/tasks/{id}` | Delete a task |
| GET | `/api/comments/task/{taskId}` | Get comments for a task |
| POST | `/api/comments` | Add a comment |
| PUT | `/api/comments/{id}` | Update a comment |
| DELETE | `/api/comments/{id}` | Delete a comment |
| GET | `/api/users/me` | Get current user profile |
| PUT | `/api/users/me` | Update profile |

SignalR hub: `ws://localhost:5000/hubs/notifications`

## License

MIT
