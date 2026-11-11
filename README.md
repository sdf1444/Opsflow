# OpsFlow

OpsFlow is a production-style internal workflow and approval platform built using React, ASP.NET Core and PostgreSQL.

The application demonstrates engineering patterns commonly found in commercial software, including:

- JWT authentication
- Role-based access control
- Workflow state management
- Audit logging
- Validation
- REST APIs
- Automated testing
- Docker
- Continuous Integration

## Technology Stack

### Frontend

- React
- TypeScript
- Vite
- Material UI
- React Query
- React Hook Form

### Backend

- ASP.NET Core (.NET 9)
- Entity Framework Core
- PostgreSQL

### DevOps

- Docker
- Docker Compose
- GitHub Actions

## Current Status

OpsFlow is an implemented request and approval workflow. Employees can create and submit requests, managers review their assigned queue, and admins can review all pending requests. New submissions are automatically assigned to the manager with the smallest active review queue.

## Running with Docker

Build and start the application:

```bash
docker compose up --build
```

Run in detached mode:

```bash
docker compose up -d
```

Frontend:

http://localhost:5173

API Swagger:

http://localhost:8080/swagger

API Health:

http://localhost:8080/health

When the development database is empty, the API seeds these local sign-in accounts:

| Role | Email | Password |
|---|---|---|
| Employee | employee@example.com | Password123! |
| Manager | manager@example.com | Password123! |
| Admin | admin@example.com | Password123! |

View logs:

```bash
docker compose logs
```

Follow API logs:

```bash
docker compose logs -f api
```

Restart only frontend:

```bash
docker compose restart frontend
```

Rebuild only API:

```bash
docker compose up --build api
```

List running containers:

```bash
docker compose ps
```

Stop the application:

```bash
docker compose down
```

Stop and remove the database volume:

```bash
docker compose down -v
```