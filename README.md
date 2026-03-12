# TrackMyGrade - Setup and Build Instructions

## Overview
TrackMyGrade is a full-stack web application built with ASP.NET Framework (backend) and Angular 18 (frontend) that allows teachers to manage and track student assessments with automatic calculations.

## Project Structure

```
TrackMyGrade/
├── TrackMyGradeAPI/          # Backend ASP.NET Framework API
│   ├── Controllers/          # REST API endpoints
│   ├── Data/                 # EF6 DbContext and SQL Server LocalDB configuration
│   ├── DTOs/                 # Data Transfer Objects (with shared StudentDtoBase)
│   ├── Handlers/             # Exception handler and validation filter
│   ├── Infrastructure/       # DI resolver and Swagger configuration
│   ├── Logging/              # ELMAH error logging configuration
│   ├── Mapping/              # AutoMapper profiles
│   ├── Models/               # Database entities
│   ├── Services/             # Business logic layer
│   ├── Validators/           # FluentValidation rules (centralized base validators)
│   ├── Program.cs            # Console entry point (OWIN self-host)
│   ├── Startup.cs            # OWIN startup and middleware pipeline
│   ├── WebApiConfig.cs       # CORS, routing and API configuration
│   ├── web.config            # Web/OWIN host configuration
│   └── Global.asax.cs        # Web project marker (startup logic in Startup.cs)
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
├── docs/                     # Project documentation
│   ├── PROJECT_REQUIREMENTS.md
│   ├── POSTMAN_INTEGRATION_GUIDE.md
│   ├── QUICK_START.md
│   ├── DELIVERABLES.md
│   ├── IMPLEMENTATION_REPORT.md
│   ├── FILE_INVENTORY.md
│   └── DAILY_REPORT_2026-03-11.md
│
├── TrackMyGradeAPI.postman_collection.json
└── TrackMyGradeAPI.postman_environment.json
```

## Backend Setup (ASP.NET Framework)

### Prerequisites
- Visual Studio 2019 or later (with .NET Framework 4.8 SDK)
- SQL Server LocalDB (included with Visual Studio)
- NuGet Package Manager

### Dependencies
The backend uses the following NuGet packages:
- `EntityFramework` v6.4.4 - ORM for database access (with SQL Server LocalDB)
- `AutoMapper` v10.1.1 - Object-to-object mapping
- `FluentValidation` v11.8.1 - Model validation
- `Microsoft.AspNet.WebApi` v5.2.9 - REST API framework
- `Microsoft.AspNet.WebApi.Cors` v5.2.9 - CORS support
- `Microsoft.AspNet.WebApi.Owin` v5.2.9 - OWIN integration
- `Microsoft.Owin` v4.2.2 - OWIN abstractions
- `Microsoft.Owin.SelfHost` v4.2.2 - Self-hosting support
- `ELMAH` v1.2.2 - Error logging and monitoring
- `Swashbuckle.Core` v5.6.0 - Swagger/OpenAPI documentation

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
   - `ApplicationDbContext.Initialize()` is called from `Startup.cs` using EF6's `CreateDatabaseIfNotExists` initializer
   - The `TrackMyGrade` database is created in SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) — no external setup required

5. **Run the API**
   ```
   Debug > Start Debugging (F5)
   Default URL: http://localhost:5000
   ```
   - Swagger UI: `http://localhost:5000/swagger`
   - The API is a self-hosted OWIN console application — it runs in a terminal window, not IIS

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

## Postman Integration

**TrackMyGrade API is now fully integrated with Postman!**

### Quick Start with Postman

1. **Import the Collection**
   - File: `TrackMyGradeAPI.postman_collection.json`
   - Contains all API endpoints with automated tests

2. **Import the Environment**
   - File: `TrackMyGradeAPI.postman_environment.json`
   - Pre-configured for `http://localhost:5000`

3. **Start Testing**
   - Run "Register Teacher" first
   - Teacher ID is automatically saved
   - All subsequent requests use saved authentication

### Features

**Complete API Coverage** - All endpoints (Teachers & Students)  
**Automated Tests** - Response validation for each request  
**Smart Authentication** - Auto-saves and applies teacher ID header  
**Test Scenarios** - Validation and error handling tests  
**Environment Variables** - Easy configuration management  
**Pre-Request Scripts** - Automated setup for each request  

### Documentation

For detailed instructions, see: **[POSTMAN_INTEGRATION_GUIDE.md](docs/POSTMAN_INTEGRATION_GUIDE.md)**

### Newman CLI (Optional)

Run tests from command line:
```powershell
# Install Newman
npm install -g newman

# Run the collection
.\TrackMyGradeAPI\run-postman-tests.ps1
```

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
- **Password Toggle**: Show/hide password visibility on login and register forms
- **Accessibility**: `aria-describedby` and `aria-invalid` attributes on form inputs

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
- Inline form validation with error messages (blur-triggered)
- Credential errors shown inline below the relevant field (not in banner)
- Global error banner for server failures
- Color-coded performance badges
- Clean, modern interface with gradient accents
- `fadeInError` animation for smooth error appearance
- Fixed-height error containers prevent layout shift

## Configuration

### Backend CORS Configuration
CORS is configured in `WebApiConfig.cs`:
```csharp
var cors = new EnableCorsAttribute("http://localhost:4200", "*", "*");
config.EnableCors(cors);
```
- Allows requests from Angular app (localhost:4200)
- Permits all HTTP methods and headers

### Swagger UI
Swashbuckle is configured in `Infrastructure/SwaggerConfig.cs` and registered from `WebApiConfig.cs`:
- **Swagger JSON**: `http://localhost:5000/swagger/docs/v1`
- **Swagger UI**: `http://localhost:5000/swagger`

XML doc comments from the compiled `TrackMyGradeAPI.XML` are loaded automatically if present.

### Error Logging (ELMAH)
ELMAH is configured in `Logging/ErrorLoggingConfig.cs` and initialized from `Startup.cs`:
- Unhandled exceptions and Web API errors are captured by `ElmahExceptionHandler` and `ElmahExceptionLogger` in `Handlers/`
- See `Logging/ELMAH_SETUP.md` for full configuration details

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
# OR run the PowerShell helper script:
.\start-api.ps1
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

## Troubleshooting

For a **comprehensive troubleshooting guide** with detailed root-cause analysis and solutions,
see [`TrackMyGradeAPI/TROUBLESHOOTING.md`](TrackMyGradeAPI/TROUBLESHOOTING.md).

### Quick Reference — Known Errors

| Error | Cause | Fix |
|-------|-------|-----|
| `Failed to listen on prefix 'http://localhost:5000/'` | Port 5000 already in use by a previous instance | Kill old process: `Get-Process -Name TrackMyGradeAPI \| Stop-Process -Force` |
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
- Entity Framework 6.4 with SQL Server LocalDB
- FluentValidation 11.8
- AutoMapper 10.1
- OWIN/Katana for self-hosting
- ELMAH for error logging
- Swashbuckle 5.6 for Swagger/OpenAPI docs

**Frontend**
- Angular 18 (Standalone Components)
- TypeScript 5.2
- RxJS 7.8
- Responsive CSS (no external UI frameworks)
- Template-driven forms

## Future Enhancements

1. **Database**: Migrate from LocalDB to a full SQL Server or PostgreSQL instance
2. **Authentication**: Implement JWT with refresh tokens
3. **Authorization**: Add role-based access control
4. **Reporting**: Export to PDF/CSV
5. **Analytics**: Performance dashboards and charts
6. **Testing**: Unit and integration tests
7. **CI/CD**: GitHub Actions or Azure DevOps pipeline
8. **Containerization**: Docker and Kubernetes deployment

## Support and Contributions

For issues or questions, refer to the detailed PRD in [`docs/PROJECT_REQUIREMENTS.md`](docs/PROJECT_REQUIREMENTS.md).

---

**Last Updated**: March 2026
**Version**: 1.1.0
