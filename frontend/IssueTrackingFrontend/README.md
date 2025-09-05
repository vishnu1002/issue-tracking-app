Users
| Endpoint | Admin | User | Rep |
| ----------------------- | ----------------- | ------------ | ----------------------- |
| GET `/api/user` | ✅ yes | ❌ no | ❌ no |
| GET `/api/user/{id}` | ✅ yes (any user) | ✅ only self | ❌ only self if allowed |
| PUT `/api/user/{id}` | ✅ yes | ✅ only self | ❌ only self if allowed |
| DELETE `/api/user/{id}` | ✅ yes | ❌ no | ❌ no |

Tickets
| Endpoint | Admin | User | Rep |
| ------------------------- | ------------- | ---------------------- | ----------------------- |
| GET `/api/ticket` | ✅ all tickets | ✅ only tickets created | ✅ only tickets assigned |
| GET `/api/ticket/{id}` | ✅ all tickets | ✅ only own | ✅ only assigned |
| POST `/api/ticket` | ✅ yes | ✅ yes | ❌ no (unless allowed) |
| PUT `/api/ticket/{id}` | ✅ yes | ✅ only own | ✅ only assigned |
| DELETE `/api/ticket/{id}` | ✅ yes | ❌ no | ❌ no |

Attachments
| Endpoint | Admin | User | Rep |
| ------------------------- | ------------- | ---------------------- | ----------------------- |
| GET `/api/ticket` | ✅ all tickets | ✅ only tickets created | ✅ only tickets assigned |
| GET `/api/ticket/{id}` | ✅ all tickets | ✅ only own | ✅ only assigned |
| POST `/api/ticket` | ✅ yes | ✅ yes | ❌ no (unless allowed) |
| PUT `/api/ticket/{id}` | ✅ yes | ✅ only own | ✅ only assigned |
| DELETE `/api/ticket/{id}` | ✅ yes | ❌ no | ❌ no |
