# TrackMyGrade - Setup and Build Instructions

## Overview
TrackMyGrade is a full-stack web application built with ASP.NET Framework (backend) and Angular 18 (frontend) that allows teachers to manage and track student assessments with automatic calculations.

## Project Structure

```
TrackMyGrade/
├── TrackMyGradeAPI/          # Backend ASP.NET Framework API
│   ├── Models/               # Database entities
│   ├── DTOs/                 # Data Transfer Objects
│   ├── Data/                 # EF Core DbContext
│   ├── Services/             # Business logic layer
│   ├── Controllers/          # REST API endpoints
│   ├── Validators/           # FluentValidation rules
│   ├── Mapping/              # AutoMapper profiles
│   ├── WebApiConfig.cs       # CORS and API configuration
│   ├── Startup.cs            # OWIN startup
│   └── Global.asax.cs        # Application lifecycle
│
├── StudentApp/               # Frontend Angular 18 SPA
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/   # Angular standalone components
│   │   │   ├── services/     # API communication services
│   │   │   ├── models/       # TypeScript interfaces
│   │   │   ├── app.routes.ts # Routing configuration
│   │   │   └── app.component.ts
│   │   ├── main.ts           # Bootstrap entry point
│   │   ├── index.html        # HTML template
│   │   └── styles.css        # Global styles
│   ├── angular.json          # Angular CLI configuration
│   ├── tsconfig.json         # TypeScript configuration
│   └── package.json          # Node dependencies
│
└── PROJECT_REQUIREMENTS.md   # Detailed PRD
```

## Backend Setup (ASP.NET Framework)

### Prerequisites
- Visual Studio 2019 or later (with .NET Framework 4.8 SDK)
- SQL Server or SQLite (we use in-memory for development)
- NuGet Package Manager

### Dependencies
The backend uses the following NuGet packages:
- `EntityFramework` v6.4.4 - ORM for database access
- `AutoMapper` v12.0.1 - Object-to-object mapping
- `AutoMapper.Extensions.Microsoft.DependencyInjection` v12.0.0
- `FluentValidation` v11.8.1 - Model validation
- `Microsoft.AspNet.WebApi` v5.2.9 - REST API framework

### Build Instructions

1. **Open the project in Visual Studio**
   ```bash
   cd TrackMyGradeAPI
   ```

2. **Restore NuGet packages**
   ```
   Right-click on project > Restore NuGet Packages
   OR use Package Manager Console:
   Update-Package
   ```

3. **Build the project**
   ```
   Build > Build Solution (Ctrl+Shift+B)
   ```

4. **Create and seed the database**
   - The database initializes automatically when the application starts
   - Database.EnsureCreated() is called in Global.asax.cs
   - Uses SQLite in-memory provider (no external setup required)

5. **Run the API**
   ```
   Debug > Start Debugging (F5)
   Default URL: http://localhost:5000
   ```

### API Endpoints

#### Teacher Endpoints
- `POST /api/teachers/register` - Register a new teacher
- `POST /api/teachers/login` - Teacher login
- `GET /api/teachers/{id}` - Get teacher profile

#### Student Endpoints
- `GET /api/students` - Get all students (for authenticated teacher)
- `GET /api/students/{id}` - Get student details
- `POST /api/students` - Create new student
- `PUT /api/students/{id}` - Update student
- `DELETE /api/students/{id}` - Delete student

## Frontend Setup (Angular 18)

### Prerequisites
- Node.js 18+ and npm 9+
- Angular CLI 18: `npm install -g @angular/cli`

### Installation

1. **Navigate to frontend directory**
   ```bash
   cd StudentApp
   ```

2. **Install dependencies**
   ```bash
   npm install
   ```

3. **Start development server**
   ```bash
   npm start
   OR
   ng serve
   ```
   - Default URL: http://localhost:4200
   - Application will auto-reload on file changes

4. **Build for production**
   ```bash
   npm run build
   OR
   ng build --configuration production
   ```
   - Output: `dist/StudentApp/browser/`

### Key Features

#### Authentication
- **Login**: Email and password authentication (plain text, demo only)
- **Registration**: Teachers can register with personal and subject information
- **Session Management**: Simple token-based (stored in localStorage)

#### Student Management
- **List View**: Table display of all students with key metrics
- **Create**: Form to add new students with 3 assessments
- **Edit**: Update existing student information
- **Delete**: Remove students with confirmation
- **Detail**: Comprehensive view of individual student performance

#### Automatic Calculations
- **Total Score**: Sum of 3 assessments (max 60)
- **Average**: Total divided by 3
- **Percentage**: (Total / 60) * 100
- **Performance Level**: 
  - Excellent: > 75%
  - Good: 56-75%
  - Satisfactory: 50-55%
  - Needs Support: < 50%

#### UI/UX Features
- Responsive design for mobile and desktop
- Inline form validation with error messages
- Global error banner for server failures
- Color-coded performance badges
- Clean, modern interface with gradient accents

## Configuration

### Backend CORS Configuration
CORS is configured in `WebApiConfig.cs`:
```csharp
var cors = new EnableCorsAttribute("http://localhost:4200", "*", "*");
config.EnableCors(cors);
```
- Allows requests from Angular app (localhost:4200)
- Permits all HTTP methods and headers

### Frontend API Base URL
In `StudentService` and `AuthService`:
```typescript
private apiUrl = 'http://localhost:5000/api/...';
```
Change this URL when deploying to production.

## Running the Full Application

### Development Mode

**Terminal 1 - Backend API**
```bash
cd TrackMyGradeAPI
# Open in Visual Studio and press F5
```

**Terminal 2 - Frontend**
```bash
cd StudentApp
npm start
```

Then navigate to `http://localhost:4200` in your browser.

### Step-by-Step Usage

1. **Register**: Go to Register page and create a teacher account
2. **Login**: Login with your credentials
3. **Add Students**: Create student records with assessments
4. **View List**: See all students in a table view
5. **Edit/Delete**: Manage student information
6. **View Details**: Check individual student performance metrics

## Data Model

### Teacher Entity
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john@example.com",
  "phone": "12345678",
  "subject": "Mathematics",
  "password": "password123",
  "token": "unique-token-guid"
}
```

### Student Entity
```json
{
  "id": 1,
  "teacherId": 1,
  "firstName": "Jane",
  "lastName": "Smith",
  "email": "jane@example.com",
  "phone": "87654321",
  "grade": 9,
  "assessment1": 18,
  "assessment2": 19,
  "assessment3": 17,
  "total": 54,
  "average": 18.0,
  "percentage": 90.0,
  "performanceLevel": "Excellent"
}
```

## Security Notes (Development Only)

⚠️ **Important**: This is a demo application with minimal security:
- Passwords stored in plain text
- No JWT or token expiration
- No authorization enforcement
- CORS allows all methods and headers

**For production**, implement:
- Password hashing (bcrypt, PBKDF2)
- JWT with refresh tokens
- Role-based access control (RBAC)
- HTTPS/TLS encryption
- Rate limiting and request validation

## Troubleshooting

For a **comprehensive troubleshooting guide** with detailed root-cause analysis and solutions,
see [`TrackMyGradeAPI/TROUBLESHOOTING.md`](TrackMyGradeAPI/TROUBLESHOOTING.md).

### Quick Reference — Known Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `No Entity Framework provider found for ADO.NET provider 'System.Data.SQLite'` | EF6 can't resolve the SQLite EF provider at runtime | Fix `providerName` in `App.config` to `System.Data.SQLite`; add code-based `SQLiteConfiguration : DbConfiguration`; apply `[DbConfigurationType]` to DbContext |
| `An error occurred while executing the command definition` (0-byte .db file) | EF6's `CreateIfNotExists()` uses SQL Server DDL that SQLite ignores | Disable default initializer with `Database.SetInitializer(null)`; create tables manually with `CREATE TABLE IF NOT EXISTS` |
| `Failed to listen on prefix 'http://localhost:5000/'` | Port 5000 already in use by a previous instance | Kill old process: `Get-Process -Name TrackMyGradeAPI \| Stop-Process -Force` |
| `Your project file doesn't list 'win' as a RuntimeIdentifier` | MSBuild can't resolve SQLite native interop DLLs without a target runtime | Add `<RuntimeIdentifier>win</RuntimeIdentifier>` to `.csproj` |
| Student form stuck on `Saving...` when creating a new student | Backend originally returned PascalCase JSON which didn't match Angular's camelCase models; this made `teacher.id` undefined and threw inside `StudentService.getHeaders()` before `subscribe()`, leaving `isSubmitting` `true` | Configure camelCase JSON in `WebApiConfig.cs`; normalize teacher objects in `AuthService`; guard `teacher?.id` in `getHeaders()`; wrap `onSubmit()` in `try/catch` and always reset `isSubmitting`; ensure Angular uses `moduleResolution: "bundler"` with workspace TypeScript |

### Backend won't start
- Ensure port 5000 is available (`netstat -ano | findstr ":5000"`)
- Check that .NET Framework 4.8 is installed
- Verify all NuGet packages are restored (`dotnet restore`)
- Kill any previous instances before restarting

### Frontend shows CORS error
- Ensure backend is running on http://localhost:5000
- Check CORS configuration in WebApiConfig.cs
- Clear browser cache and localStorage

### API calls fail
- Check browser Network tab in DevTools
- Verify API base URL matches backend port
- Ensure teacher header (X-TeacherId) is sent

### Login/Registration fails
- Check backend is running and responding
- Verify email format and validation rules
- Check browser console for detailed errors

## Technology Stack

**Backend**
- C# with .NET Framework 4.8
- ASP.NET Web API 5.2
- Entity Framework 6.4
- FluentValidation 11.8
- AutoMapper 12.0
- OWIN/Katana for self-hosting

**Frontend**
- Angular 18 (Standalone Components)
- TypeScript 5.2
- RxJS 7.8
- Responsive CSS (no external UI frameworks)
- Template-driven forms

## Future Enhancements

1. **Database**: Replace in-memory with SQL Server or PostgreSQL
2. **Authentication**: Implement JWT with refresh tokens
3. **Authorization**: Add role-based access control
4. **Reporting**: Export to PDF/CSV
5. **Analytics**: Performance dashboards and charts
6. **Testing**: Unit and integration tests
7. **CI/CD**: GitHub Actions or Azure DevOps pipeline
8. **Containerization**: Docker and Kubernetes deployment

## Support and Contributions

For issues or questions, refer to the detailed PRD in `PROJECT_REQUIREMENTS.md`.

---

**Last Updated**: February 2026
**Version**: 1.0.0
