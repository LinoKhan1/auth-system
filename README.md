# Auth System Full-Stack Project

A **full-stack authentication system** built with **Next.js (TypeScript)** for the frontend and **ASP.NET Core Web API (C#)** for the backend, using **PostgreSQL** as the database. The backend is fully Dockerized, while the frontend runs locally during development.

This project demonstrates:

- User registration and login
- User Details View
- JWT-based authentication
- Secure password storage (BCrypt)
- Protected routes in frontend
- Dockerized backend + PostgreSQL
- Clean, production-ready architecture

---
``` bash
auth-system/
├── client/               # Next.js frontend (TypeScript)
│   ├── app/
│   │   ├── login/
│   │   ├── register/
│   │   └── user/
│   ├── features/
│   │   ├── auth/         # components, hooks, services, types, context
│   │   └── user/         # components, services, types
│   └── libs/             # apiClient.ts
├── server/               # ASP.NET Core Web API backend (C#)
│   ├── Presentation/     # controllers, dtos, mappings
│   ├── Application/      # interfaces, services
│   ├── Domain/           # entities
│   └── Infrastructure/   # data, authentication, repository
├── server.tests/         # unit tests for backend
│   ├── Services/         # tests for Application services (e.g., JwtAuthService)
│   ├── Repositories/     # tests for repositories (data access layer)
│   └── Controllers/      # tests for API controllers (AuthController, etc.)
├── docker-compose.yml    # Backend + PostgreSQL orchestration
└── README.md
```
---

## 🖥 Frontend (Next.js)

### Features

- Registration page: first name, last name, email, password
- Login page: email + password
- User page: displays first name, last name, email (protected)
- Axios client configured for API requests
- Middleware protects routes requiring authentication
- Global authentication state via AuthContext:
- Manages user, loading, and error state across all components
- Provides register, login, logout, and fetchUser methods
- Ensures authenticated pages redirect to login if user is not logged in
- Session maintained using HTTP-only cookies set by the backend

### Environment Variables

Set env variables:

```bash
NEXT_PUBLIC_API_URL=""
```

### Run Frontend

```bash
cd client
npm install
npm run dev
```
## Backend (ASP.NET Core Web API)

### Features

- Register user (hashes password with BCrypt)
- Login user (validates credentials and issues JWT)
- JWT stored in HTTP-only cookie
- Protected endpoint /api/users/me returns authenticated user details
- PostgreSQL database for user storage
- EF Core migrations for database schema
- Clean architecture: Presentation → Application → Domain → Infrastructure

### Environment Variables

Set env variables:

```bash
# Database
DB_HOST=""
DB_PORT=""
DB_NAME=""
DB_USER=""
DB_PASSWORD=""

# JWT
JWT_SECRET_KEY=""
JWT_ISSUER=""
JWT_AUDIENCE=""
JWT_EXPIRE_HOURS=""
```

### Docker Setup

This project includes a Dockerized development environment using `docker-compose.yml`, which manages both the backend API and PostgreSQL database.

#### Services

- **api (ASP.NET Core backend)**
  - Built from `server/Dockerfile`
  - Listens on port **8080**
  - Includes **dotnet-ef CLI** to run **Entity Framework Core migrations** inside the container
  - `db.Database.Migrate()` is automatically called on startup to ensure tables are created

- **postgres (PostgreSQL database)**
  - Image: `postgres:18`
  - Listens on port **5432**
  - Uses volume `pgdata` to **persist database data**
  - Can be initialized with SQL scripts in `docker/postgres-init`

#### Key Features

- **Migrations included in Docker image**
  - Migrations located in `server/Infrastructure/Data/Migrations` are compiled into the published assembly
  - Allows `dotnet ef database update` or automatic `db.Database.Migrate()` to create/update tables in `authdb`

- **Environment Variables**
  - Configurable via a `.env` file at the project root
  - Includes DB connection, JWT settings, and other secrets
  - Avoids hardcoding sensitive info in `docker-compose.yml`

#### Commands

- **Build images:**  

```bash
Build and start backend + PostgreSQL using Docker:
docker compose up --build
API will be available at http://localhost:8080.
Swagger UI available at http://localhost:8080/swagger.
```

#### Run EF Core Migrations

```bash
docker compose run --rm server dotnet ef database update --project server/server.csproj --startup-project server/server.csproj
```
- **For Local**  
```bash
dotnet restore
dotnet run
```

### Authentication Flow

- User registers via /register → backend hashes password and stores in DB.
- User logs in via /login → backend validates credentials → issues JWT → stored in HTTP-only cookie.
- Protected endpoint /users/me → backend validates JWT → returns user info.
- Frontend middleware ensures only authenticated users can access protected pages.


## Technologies Used

### Frontend:

- Next.js 16+
- TypeScript
- Axios
- React Hooks

### Backend:

- ASP.NET Core 10
- C#
- Entity Framework Core
- PostgreSQL
- JWT Authentication
- BCrypt for password hashing
- Docker

### Tools:

- VS Code
- Docker Desktop
- Postman (for API testing)

## Testing

- Unit tests: AuthService, UserService, JWT generation
- Integration tests (optional): registration → login → protected endpoint
- Frontend tests can use Jest + React Testing Library

### Run Tests

```bash
npm run test
dotnet test


```

## Development Workflow

```bash
Start backend & DB in Docker:
docker compose up --build
Start frontend locally:
cd client
npm run dev
Access frontend at http://localhost:3000.
Use Postman or frontend UI to test registration, login, and protected routes.
```

## Production Considerations

- Store secrets in environment variables or secret managers
- Enable HTTPS and secure cookies
- Configure proper CORS settings for production domains
- Consider containerizing frontend for full Docker deployment

