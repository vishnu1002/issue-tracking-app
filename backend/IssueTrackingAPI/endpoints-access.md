# API Endpoints - Role-Based Access Control

## Overview

This document lists all available API endpoints with their role-based access control. The system supports three user roles:

- **Admin**: Full system access
- **User**: Can create and manage their own tickets
- **Rep (Representative)**: Can manage assigned tickets and unassigned tickets

---

## 🔐 Authentication Endpoints

| Method | Endpoint          | Access        | Description                    |
| ------ | ----------------- | ------------- | ------------------------------ |
| POST   | `/api/auth/login` | **Anonymous** | User login - returns JWT token |

---

## 👥 User Management Endpoints

| Method | Endpoint         | Access            | Description                           |
| ------ | ---------------- | ----------------- | ------------------------------------- |
| GET    | `/api/user`      | **Admin Only**    | Get all users                         |
| GET    | `/api/user/{id}` | **Authenticated** | Get user by ID (own profile or Admin) |
| POST   | `/api/user`      | **Anonymous**     | Create new user (registration)        |
| PUT    | `/api/user/{id}` | **Authenticated** | Update user (own profile or Admin)    |
| DELETE | `/api/user/{id}` | **Admin Only**    | Delete user                           |

**Note**: Users can only view/update their own profile unless they are Admin.

---

## 🎫 Ticket Management Endpoints

| Method | Endpoint             | Access            | Description                             |
| ------ | -------------------- | ----------------- | --------------------------------------- |
| GET    | `/api/ticket`        | **Authenticated** | Get tickets (role-based filtering)      |
| GET    | `/api/ticket/search` | **Authenticated** | Search tickets with filters             |
| GET    | `/api/ticket/{id}`   | **Authenticated** | Get ticket by ID                        |
| POST   | `/api/ticket`        | **User, Admin**   | Create new ticket                       |
| PUT    | `/api/ticket/{id}`   | **Authenticated** | Update ticket (role-based restrictions) |
| DELETE | `/api/ticket/{id}`   | **Admin Only**    | Delete ticket                           |

### Role-Based Ticket Access:

- **Admin**: Can see all tickets
- **User**: Can see only tickets they created
- **Rep**: Can see tickets assigned to them + unassigned tickets

### Role-Based Ticket Updates:

- **User**: Can only update tickets they created
- **Rep**: Can update tickets assigned to them + unassigned tickets
- **Admin**: Can update any ticket

---

## 📎 Attachment Management Endpoints

| Method | Endpoint               | Access            | Description                             |
| ------ | ---------------------- | ----------------- | --------------------------------------- |
| GET    | `/api/attachment`      | **Admin Only**    | Get all attachments                     |
| GET    | `/api/attachment/{id}` | **Authenticated** | Get attachment by ID                    |
| POST   | `/api/attachment`      | **Authenticated** | Create new attachment                   |
| DELETE | `/api/attachment/{id}` | **Authenticated** | Delete attachment (own ticket or Admin) |

**Note**: Users can only delete attachments from tickets they created, unless they are Admin.

---

## 📊 Dashboard Endpoints (Admin Only)

| Method | Endpoint                     | Access         | Description                            |
| ------ | ---------------------------- | -------------- | -------------------------------------- |
| GET    | `/api/dashboard/stats`       | **Admin Only** | Get dashboard statistics               |
| GET    | `/api/dashboard/trends`      | **Admin Only** | Get ticket trends (default 30 days)    |
| GET    | `/api/dashboard/performance` | **Admin Only** | Get representative performance metrics |

---

## 🔔 Notification Endpoints

| Method | Endpoint                          | Access            | Description                          |
| ------ | --------------------------------- | ----------------- | ------------------------------------ |
| GET    | `/api/notifications`              | **Authenticated** | Get user's notifications (paginated) |
| GET    | `/api/notifications/unread-count` | **Authenticated** | Get unread notification count        |
| PUT    | `/api/notifications/{id}/read`    | **Authenticated** | Mark notification as read            |
| PUT    | `/api/notifications/read-all`     | **Authenticated** | Mark all notifications as read       |

**Note**: Users can only access their own notifications.

---

## 📈 KPI & Analytics Endpoints

| Method | Endpoint                           | Access            | Description                           |
| ------ | ---------------------------------- | ----------------- | ------------------------------------- |
| GET    | `/api/kpi/representative/{id}`     | **Authenticated** | Get representative KPI (own or Admin) |
| GET    | `/api/kpi/representatives`         | **Admin Only**    | Get all representatives KPI           |
| GET    | `/api/kpi/average-resolution-time` | **Admin Only**    | Get average resolution time           |
| GET    | `/api/kpi/total-resolved`          | **Admin Only**    | Get total tickets resolved            |

**Note**: Representatives can only view their own KPI, Admins can view any representative's KPI.

---

## 🔌 SignalR Hub

| Endpoint           | Access            | Description                 |
| ------------------ | ----------------- | --------------------------- |
| `/notificationHub` | **Authenticated** | Real-time notifications hub |

### SignalR Methods:

- `JoinUserGroup(userId)` - Join personal notification group
- `LeaveUserGroup(userId)` - Leave personal notification group
- `JoinAdminGroup()` - Join admin notification group
- `LeaveAdminGroup()` - Leave admin notification group

### SignalR Events:

- `ReceiveNotification` - Receive real-time notifications

---

## 🏥 Health Check

| Method | Endpoint  | Access     | Description           |
| ------ | --------- | ---------- | --------------------- |
| GET    | `/health` | **Public** | Health check endpoint |

---

## 🔍 Search Parameters

### Ticket Search (`/api/ticket/search`):

- `title` - Search by ticket title
- `description` - Search by description
- `priority` - Filter by priority (Low, Medium, High)
- `type` - Filter by type (Software, Hardware)
- `status` - Filter by status (Open, Closed, In Progress)
- `createdByUserId` - Filter by creator
- `assignedToUserId` - Filter by assignee
- `createdFrom` - Filter by creation date (from)
- `createdTo` - Filter by creation date (to)
- `updatedFrom` - Filter by update date (from)
- `updatedTo` - Filter by update date (to)
- `pageNumber` - Page number (default: 1)
- `pageSize` - Page size (default: 10)
- `sortBy` - Sort field (CreatedAt, UpdatedAt, Priority, Status)
- `sortOrder` - Sort order (asc, desc)

### KPI Date Filters:

- `fromDate` - Start date for KPI calculations
- `toDate` - End date for KPI calculations

---

## 🚨 Error Responses

All endpoints return consistent error responses:

```json
{
  "status": 400,
  "message": "Error description",
  "detail": "Detailed error information"
}
```

Common HTTP status codes:

- `200` - Success
- `201` - Created
- `400` - Bad Request
- `401` - Unauthorized
- `403` - Forbidden
- `404` - Not Found
- `500` - Internal Server Error

---

## 🔑 Authentication

All protected endpoints require a valid JWT token in the Authorization header:

```
Authorization: Bearer <jwt_token>
```

JWT tokens include:

- User ID
- User Name
- User Email
- User Role
- Expiration time (default: 60 minutes)

---

## 📝 Notes

1. **Role-Based Filtering**: Many endpoints apply role-based filtering automatically
2. **Real-time Notifications**: Ticket operations trigger real-time notifications via SignalR
3. **KPI Tracking**: Ticket resolution automatically calculates KPI metrics
4. **File Attachments**: Attachment operations are tied to ticket ownership
5. **Pagination**: List endpoints support pagination for better performance
6. **Search**: Advanced search with multiple filters and sorting options
