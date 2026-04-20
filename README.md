# 🚀 SmartTasks

A modern task management app inspired by Jira and Trello, built to help teams organize, track, and complete work efficiently.

---

## 📌 1. Overview

SmartTasks is a collaborative project management platform consisting of:

- **Backend API** (`SmartTasksAPI`) — handles business logic, authentication, and data persistence  
- **Frontend** (`smarttasks-frontend`) — provides the user interface and client-side functionality  

---

## ⚙️ 2. Core Features

### 👤 Users
- Register and authenticate (JWT-based)
- Basic user management (`/api/users`)

### 📋 Boards
- Create, update, delete boards (`/api/boards`)
- Manage members and ownership

### 📑 Lists
- Organize boards into columns
- Maintain order using positions

### 📝 Cards
- Create, update, move, and delete tasks
- Support for descriptions, due dates, assignments, and comments

### 💬 Comments
- Add, view, and delete comments on cards

---

## 🧑‍💻 3. Team & Roles

| Student Name                  | Primary Role                         | GitHub Username    |
|-------------------------------|--------------------------------------|--------------------|
| Mustață Teodora               | Team Lead / DevOps / Infrastructure  | teodora-mustata    |
| Peleș Ștefania                | Frontend Developer                   | PelesStefania      |
| Radea Constantin-Sebastian    | Backend Developer                    | Sebi23647          |
| Pleșu Iulia                   | QA Engineer / Tester                 | Iulia-plesu        |

## 🛠️ 4. Tech Stack

- **Backend:** ASP.NET Core (.NET 8), Entity Framework Core, PostgreSQL (Npgsql)
- **Frontend:** Angular (served via nginx in Docker)
- **Auth:** JWT Authentication
- **Other:** Docker, Swagger

---

## 📂 5. Project Structure

- `SmartTasksAPI/` — backend (controllers, services, EF Core, etc.)
- `smarttasks-frontend/` — Angular frontend
- `SmartTasksAPI.Tests/` — unit & integration tests
- `docker-compose.yml` — container setup

---

## ▶️ 6. Setup

### 📦 Requirements
- .NET SDK 8.0
- Docker
- PostgreSQL

### ▶️ Run with Docker
In the repository root run:
```bash
docker compose up --build