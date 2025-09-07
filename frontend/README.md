# Issue Tracking - Frontend (Angular 20)

Angular 20 SPA that consumes the Issue Tracking API and provides dashboards, ticket management, and analytics.

## App Architecture

```mermaid
graph TD
  Router[Angular Router] -->|routes| Pages[Pages]
  Pages --> Components[Components]
  Pages --> Services[Services]
  Services --> Http[HTTP Client]
  Http --> API[IssueTrackingAPI]
```

Key areas:

- Pages under `src/app/pages` for features (dashboard, tickets, analytics)
- Services under `src/app/core/services` for API calls and state
- Models under `src/app/models` for DTOs/interfaces

## User Workflow (Frontend)

```mermaid
flowchart LR
  A[Login] --> B[Dashboard]
  B --> C[View My Tickets]
  C --> D[Filter/Sort/Search]
  D --> E[Open Ticket Detail]
  E --> F[Comment/Update Status]
  F --> G[Return to List]
  B --> H[Analytics]
```

## Frontend Routes

Frontend routing map (from `frontend/IssueTrackingFrontend/src/app/app.routes.ts`):

| Path                   | Component       | Guards      | Roles         |
| ---------------------- | --------------- | ----------- | ------------- |
| `/`                    | `Landing`       | -           | -             |
| `/login`               | `Login`         | -           | -             |
| `/register`            | `Register`      | -           | -             |
| `/dashboard`           | `Dashboard`     | `authGuard` | Authenticated |
| `/dashboard/tickets`   | `Tickets`       | inherits    | Authenticated |
| `/dashboard/profile`   | `Profile`       | inherits    | Authenticated |
| `/dashboard/users`     | `Users`         | `roleGuard` | Admin         |
| `/dashboard/analytics` | `Analytics`     | `roleGuard` | Admin         |
| `**`                   | redirect to `/` | -           | -             |

## Environment

- Configure API base in `src/env/env.ts`.
- Interceptors for auth in `src/app/core/interceptors`.

## Development

```bash
ng serve
```

## Build

```bash
ng build
```

Artifacts in `dist/` can be served via any static server.
