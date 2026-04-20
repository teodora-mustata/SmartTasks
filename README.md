# SmartTasks

A powerful task management app that takes to-do lists to the next level, similar to Jira or Trello, helping teams organize, track, and complete tasks efficiently.

---

## 1. Description & Objectives

SmartTasks provides a full-featured collaborative task and project management platform. The app is split between a backend API (`SmartTasksAPI`) and a frontend SPA (`smarttasks-frontend`). The main objectives are to provide teams with an easy-to-use system for creating and organizing work, enabling collaboration and visibility across projects.

Core functionality (what the app does):

- Users
  - Register and authenticate (JWT-based authentication available via the backend `api/auth` endpoints).
  - Basic user management endpoints (list users via `GET /api/users`, create users via `POST /api/users` — used by the frontend registration flow).

- Boards
  - Create, read, update, delete project boards (`/api/boards`).
  - Board ownership and membership management: add/remove members (`POST /api/boards/{boardId}/members`, `DELETE /api/boards/{boardId}/members/{userId}`). Owners are tracked and cannot be removed without reassigning ownership.

- Lists (columns)
  - Create and manage lists for a specific board (`/api/boards/{boardId}/lists`, `GET /api/lists/{listId}`, `PUT /api/lists/{listId}`, `DELETE /api/lists/{listId}`).
  - Lists maintain a `Position` for ordering within a board.

- Cards (tasks)
  - Create, update, move, and delete cards within lists (`/api/lists/{listId}/cards`, `GET /api/cards/{cardId}`, `PUT /api/cards/{cardId}`, `PATCH /api/cards/{cardId}/move`, `DELETE /api/cards/{cardId}`).
  - Cards support title, description, due date, position ordering, assignments and comments.
  - Assign/unassign users to/from cards (`POST /api/cards/{cardId}/assignments/{userId}`, `DELETE /api/cards/{cardId}/assignments/{userId}`).

- Comments
  - Add and remove comments on cards and list comments by card (`POST /api/cards/{cardId}/comments`, `GET /api/cards/{cardId}/comments`, `DELETE /api/comments/{commentId}`).

- Data & persistence
  - Backend persists entities (Users, Boards, BoardLists, Cards, Assignments, Comments, BoardMembers) using Entity Framework Core with PostgreSQL (`Npgsql`).
  - Migrations are applied automatically at startup (Program.cs attempts migrations / EnsureCreated with retry logic).

- Frontend
  - Angular-based SPA (in `smarttasks-frontend`) with pages for authentication (login/register) and client-side handling of users and sessions.
  - The frontend currently uses a localStorage-backed mock auth for UX while also attempting to persist created users to the API.
  - `nginx.conf` and Docker files are included for serving the frontend in production/staging containers.

- Authentication & security
  - Backend supports JWT authentication (configured in `Program.cs`) and exposes `api/auth` endpoints for register, login and `api/auth/me` to inspect authenticated user details.
  - Password hashing is supported server-side via `IPasswordHasher<User>`.

- Tests
  - `SmartTasksAPI.Tests` contains unit and integration tests covering controllers, services, repository logic and DTO validation (see test files: controller/service tests, repository tests and integration test helpers).

- DevOps / Containerization
  - Dockerfiles and `docker-compose.yml` are included for local dev and containerized runs (API, PostgreSQL, frontend). Environment templates (`.env`) are present for configuration.

Where to look in the repo:

- `SmartTasksAPI/` — backend project: controllers, services, repositories, models, EF Core `ApplicationDbContext`, Dockerfile and launch settings.
- `smarttasks-frontend/` — Angular frontend: pages (`login`, `register`, etc.), core services (auth), nginx config.
- `SmartTasksAPI.Tests/` — unit and integration tests for the backend.
- `docker-compose.yml` and top-level `.env` — compose and environment for local containerized setup.

This section replaces the previous short objectives text with a concise functional summary of all implemented features. See the rest of this README for setup and usage instructions.

---

## 2. Team & Roles

| Student Name                  | Primary Role                         | GitHub Username    |
|-------------------------------|--------------------------------------|--------------------|
| Mustață Teodora               | Team Lead / DevOps / Infrastructure  | teodora-mustata    |
| Peleș Ștefania                | Frontend Developer                   | PelesStefania      |
| Radea Constantin-Sebastian    | Backend Developer                    | Sebi23647          |
| Pleșu Iulia                   | QA Engineer / Tester                 | Iulia-plesu        |

---

## 3. Architecture & Technologies

SmartTasks is split into two main parts:

- `SmartTasksAPI` - Backend service built with `ASP.NET Core (.NET 8)`, using `Entity Framework Core` with `Npgsql` for PostgreSQL. Authentication is JWT-based. Swagger is enabled for API exploration.
- `smarttasks-frontend` - Frontend application served via a web server (nginx in Docker config).

Key technologies:

- .NET 8 (`net8.0`) for the API
- Entity Framework Core + Npgsql (PostgreSQL)
- JWT Bearer Authentication
- Docker for containerized development
- Swagger for API docs

---

## 4. Local Setup (How to Run)

Prerequisites:

- .NET SDK 8.0
- Docker
- PostgreSQL

Running with Docker:

1. Ensure Docker Desktop is running.
2. From the repository root run:
   - `docker compose up --build`
3. This will bring up the API, database, and frontend.

---

