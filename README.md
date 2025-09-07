# Issue Tracking App

A full‑stack issue tracking system with a .NET 9 Web API backend and an Angular 20 frontend. It supports authentication, ticket lifecycle management, KPIs/analytics, and role‑based access.

## Combined Architecture (Frontend + Backend)

The app is split into two deployable units: `backend/IssueTrackingAPI` and `frontend/IssueTrackingFrontend`. Data persists in a relational database via Entity Framework Core.

```mermaid
flowchart LR
  User --> Frontend
  Frontend --> Router
  Frontend --> Services
  Frontend --> Pages
  Services --> API
  API --> Controllers
  Controllers --> Repositories
  Repositories --> EFCore
  EFCore --> Database
  API --> Middleware

  subgraph Angular Frontend
    Frontend
    Router
    Services
    Pages
  end
  subgraph ASP.NET Backend
    API
    Controllers
    Repositories
    EFCore
    Middleware
  end
  Database[(Relational DB)]
```

## User Interaction Workflow

```mermaid
flowchart LR
  Login --> Dashboard
  Dashboard --> CreateTicket
  CreateTicket --> AssignPriority
  AssignPriority --> InProgress
  InProgress --> CommentUpdate
  CommentUpdate --> ResolveClose
  ResolveClose --> KPIsAnalytics
```

## Page Workflows

### Landing

```mermaid
flowchart LR
  Landing --> Login
  Landing --> Register
```

### Login

```mermaid
flowchart LR
  EnterCredentials --> Validate
  Validate --> Success
  Validate --> Failure
  Success --> NavigateDashboard
  Failure --> ShowError
```

### Register

```mermaid
flowchart LR
  FillForm --> Submit
  Submit --> CreateUser
  CreateUser --> Login
  Login --> NavigateDashboard
```

### Dashboard → Profile

```mermaid
flowchart LR
  LoadProfile --> EditFields
  EditFields --> SaveProfile
  SaveProfile --> Confirm
```

### Dashboard → Users (Admin)

```mermaid
flowchart LR
  ViewUsers --> CreateUser
  ViewUsers --> EditUser
  ViewUsers --> DeleteUser
  CreateUser --> RefreshUsers
  EditUser --> RefreshUsers
  DeleteUser --> RefreshUsers
```

### Dashboard → Analytics (Admin)

```mermaid
flowchart LR
ReviewKPIs
```

## Tech Stack

- Backend: .NET 9, ASP.NET Core, EF Core, SQL (migrations in `Migrations/`)
- Frontend: Angular 20, TypeScript, Tailwind CSS
- Auth: JWT Token

## Monorepo Structure

```text
backend/
  IssueTrackingAPI/      # Web API
  IssueTrackingTest/     # Tests
frontend/
  IssueTrackingFrontend/ # Angular app
```

## Getting Started

### Prerequisites

- Node 20+, NPM
- .NET SDK 9.0
- SSMS SQL database server

### Backend

1. Update connection string in `backend/IssueTrackingAPI/appsettings.json`.
2. Apply migrations and run:
   - `dotnet ef database update`
   - `dotnet run --project backend/IssueTrackingAPI`

API will listen on the configured port (see `Properties/launchSettings.json`).

### Frontend

1. Install dependencies: `cd frontend/IssueTrackingFrontend && npm install`
2. Configure API base URL in `src/env/env.ts` if needed.
3. Start dev server: `ng serve`

### Environment Variables

- Backend: connection string, JWT key, CORS origins
- Frontend: API base URL

## Workflows & Roles

- Roles: Admin, Manager, Agent, Reporter
- Ticket lifecycle: New → Assigned → In Progress → Resolved → Closed
