# Starship Management System

A full-stack application for managing Star Wars starships with user authentication. Built with .NET 8 Web API backend and React + TypeScript frontend.

## 🏗️ Architecture

- **Backend**: ASP.NET Core Web API (.NET 8)
- **Frontend**: React 19 + TypeScript + Vite
- **Database**: SQL Server (LocalDB)
- **Authentication**: JWT with HTTP-only cookies
- **Testing**: xUnit (Backend), Vitest (Frontend)

## 📋 Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js](https://nodejs.org/) (v18 or higher)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (included with Visual Studio)
- A code editor (VS Code, Visual Studio, etc.)

## Security Note

⚠️ **For Production**: The JWT secret in `appsettings.json` is included for evaluation purposes only. In a production environment, this would be:
- Stored in Azure Key Vault / AWS Secrets Manager
- Set via environment variables in CI/CD
- Managed with `dotnet user-secrets` during local development
- Never committed to source control

## 🚀 Setup Instructions

### 1. Clone the Repository

```bash
git clone <repository-url>
cd GoEngineerExercise
```

### 2. Backend Setup (StarshipAPI)

#### Install Dependencies & Setup Database

```bash
cd StarshipAPI
dotnet restore
```

#### Run Database Migrations

```bash
dotnet ef database update
```

This creates the database with two tables:
- `Starships` - Starship data
- `Users` - User authentication

#### Start the API Server

```bash
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5210`
- Swagger UI: `http://localhost:5210/swagger`

**Note**: The database will be automatically seeded with sample starships on first run.

### 3. Frontend Setup (StarshipClient)

Open a new terminal window:

```bash
cd StarshipClient
npm install
npm run dev
```

The client will be available at: `http://localhost:5173`

## 🧪 Running Tests

### Backend Tests

```bash
cd StarshipAPI.Tests
dotnet test
```

Run with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage" --settings coverage.runsettings
reportgenerator -reports:"TestResults/**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:Html
start coveragereport/index.html
```

### Frontend Tests

```bash
cd StarshipClient
npm test
```

Run with coverage:
```bash
npm run test:coverage
```

Coverage report will be in `StarshipClient/coverage/index.html`

## 📱 Using the Application

### First Time Setup

1. Start both the backend API and frontend client (see Setup Instructions above)
2. Navigate to `http://localhost:5173`
3. Register a new account:
   - Click "Don't have an account? Register"
   - Enter username, email, and password
   - Click "Register"
4. You'll be automatically logged in after registration

### Features

- **Authentication**: Secure JWT-based authentication with HTTP-only cookies
- **Starship Management**: View, add, edit, and delete starships
- **Responsive UI**: Built with Tailwind CSS for mobile and desktop
- **Protected Routes**: Automatic redirect to login for unauthenticated users

## 🗂️ Project Structure

```
GoEngineerExercise/
├── StarshipAPI/                 # .NET Web API Backend
│   ├── Controllers/            # API endpoints
│   ├── Services/               # Business logic
│   ├── Data/                   # EF Core DbContext
│   ├── Models/                 # Data models
│   ├── Interfaces/             # Service interfaces
│   └── Migrations/             # Database migrations
│
├── StarshipAPI.Tests/          # Backend unit tests
│   └── *Tests.cs              # xUnit test files
│
└── StarshipClient/             # React Frontend
    ├── src/
    │   ├── components/        # React components
    │   ├── api/              # API service layer
    │   ├── hooks/            # Custom React hooks
    │   └── models/           # TypeScript types
    └── *.test.tsx            # Vitest test files
```

## 🔧 Configuration

### Backend (appsettings.json)

- **ConnectionStrings:DefaultConnection**: SQL Server connection string
- **Jwt:Secret**: JWT signing key (change in production!)
- **Jwt:Issuer**: Token issuer
- **Jwt:Audience**: Token audience

### Frontend (src/api/apiClient.ts)

- **baseURL**: API base URL (default: `http://localhost:5210/api`)
- **withCredentials**: Enables cookie handling for authentication

## 🔐 Authentication Flow

1. User registers or logs in
2. API generates JWT token and sets it as HTTP-only cookie
3. Frontend automatically includes cookie in all subsequent requests
4. Protected routes check authentication status on mount
5. User can logout, clearing the authentication cookie

## 📊 Test Coverage

The project maintains high test coverage:

- **Backend**:  97% line coverage, 90% branch coverage (Controllers, Services, Data Layer)
- **Frontend**: 98.68% statement coverage, 96.15% branch coverage (Components, Hooks, Services)

Tests focus on:
- Unit tests for individual components/services
- Integration tests for API endpoints
- UI interaction tests for user flows

## 🛠️ Key Technologies

### Backend
- ASP.NET Core Web API
- Entity Framework Core
- JWT Authentication
- BCrypt for password hashing
- Swagger/OpenAPI

### Frontend
- React 19
- TypeScript
- Vite (build tool)
- Axios (HTTP client)
- Tailwind CSS (styling)
- Headless UI (accessible components)
- Vitest + Testing Library (testing)

## 📝 Assumptions & Design Decisions

### Authentication
- JWT tokens stored in HTTP-only cookies for security
- Session validation on page load for persistent login
- Tokens are not exposed to JavaScript (XSS protection)

### Data Model
- Starships follow SWAPI schema for compatibility
- User model includes username and email for better UX
- All starship fields are optional except name, model, manufacturer

### Frontend Architecture
- Context API for global auth state (avoids prop drilling)
- Custom hooks for data fetching (separation of concerns)
- Integration tests preferred over mocking complex components

### Testing Strategy
- Focus on behavior over implementation
- Minimal CSS/styling tests (not business logic)
- Mock external dependencies (API, Auth context)
- Integration tests for critical user flows

### Error Handling
- API errors logged to console for debugging
- User-friendly error messages in UI
- Form validation prevents invalid submissions
- Confirmation dialogs for destructive actions

## 🐛 Troubleshooting

### Database Connection Issues
```bash
# Check LocalDB is running
sqllocaldb info mssqllocaldb

# Start LocalDB if needed
sqllocaldb start mssqllocaldb
```

### Port Already in Use
- Backend: Change port in `Properties/launchSettings.json`
- Frontend: Vite will suggest an alternate port automatically

### CORS Errors
- Verify API is running on `http://localhost:5210`
- Check CORS configuration in `Program.cs` allows `http://localhost:5173`

### Authentication Issues
- Clear browser cookies and restart both servers
- Verify JWT secret is set in `appsettings.json`
- Check browser console for detailed error messages