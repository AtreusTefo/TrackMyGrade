# Project Requirements Document (PRD)

## Project Name
TrackMyGrade

## Purpose
Provide a responsive, multi-role web application for managing student assessments. Teachers can register, log in, and manage student records with automatic scoring calculations. Students can log in and view their own performance dashboard. Administrators can manage teachers, students, and view audit logs.

## Stakeholders
- **Admins** — system administrators with full oversight
- **Teachers** — primary academic users
- **Students** — self-service portal users

## Scope

### In Scope
- Three-role system: Admin, Teacher, Student
- Teacher registration and login
- Student CRUD (create, read, update, delete) — managed by teachers
- Automatic calculation of totals, averages, percentages, and performance levels
- Student self-service login and dashboard (view own profile and marks)
- Student assessment self-submission portal
- Admin login, dashboard with audit log, teacher/student management tabs
- Responsive UI with form validation and error messaging
- REST API for all operations
- ELMAH-based error logging
- Swagger UI for API documentation

### Out of Scope
- Password hashing and JWT-based authentication (planned future enhancement)
- Class/subject taxonomy beyond a simple subject string
- PDF/CSV reporting and analytics dashboards
- Multi-tenancy beyond teacher-scoped student isolation

---

## Functional Requirements

### Admin
1. Admins log in via `/admin` with email and password.
2. Admin dashboard displays tabs for Teachers, Students, and Audit Logs.
3. Admins can view all teacher and student records.
4. Audit logs record entity-level changes (create, update, delete) with timestamps.

### Teacher Registration
1. Teachers register with: first name, last name, email, phone, subject, password.
2. Registration validates required fields and formats on both client and server.
3. After successful registration, the UI redirects to the teacher login page.

### Teacher Login
1. Teachers log in with email and password via `/login`.
2. Invalid credentials return a friendly inline error.
3. Login returns a token and teacher profile data stored in `localStorage`.

### Student Management (Teacher)
1. Teachers create a student with:
   - First name, last name, email, phone
   - Omang number or passport number (`OmangOrPassport`, 9 alphanumeric characters)
   - Grade (integer, 1–12)
   - Assessment1, Assessment2, Assessment3 (0–20 each)
   - Initial password (for student self-service login)
2. The system auto-generates a unique `StudentNumber` (format: `STU-YYYY-NNNN`).
3. Students are listed in a table view with key identifiers and performance badges.
4. Teachers can view detailed student information including all scores.
5. Teachers can update student records.
6. Teachers can delete student records with a confirmation dialog.

### Student Self-Service Portal
1. Students log in at `/student-login` using their registered email and password.
2. On successful login, a token is stored in `localStorage` via `StudentAuthService`.
3. The student dashboard (`/student-dashboard`) displays:
   - Full personal profile (name, email, phone, grade, student number)
   - Assessment scores (Assessment 1, 2, 3)
   - Calculated metrics: total, average, percentage, performance level
4. Students may submit their own assessment scores via the `submit-assessments` endpoint.

### Calculations
1. Total = Assessment1 + Assessment2 + Assessment3 (max 60)
2. Average = Total / 3
3. Percentage = (Total / 60) × 100
4. Performance Level:
   - Needs Support: < 50%
   - Satisfactory: 50–55%
   - Good: 56–75%
   - Excellent: > 75%

### Validation Rules
- Names: required, 2–50 characters, letters only
- Email: required, valid email format
- Phone: required, exactly 8 digits
- OmangOrPassport: required, exactly 9 alphanumeric characters (`^[a-zA-Z0-9]{9}$`)
- Subject: required, max 100 characters
- Password: required, 6–20 characters
- Grade: required, integer 1–12
- Assessments (1–3): required, integer, 0–20 inclusive

---

## Non-Functional Requirements

### Performance
- Typical API responses should complete within 500 ms in development.

### Usability
- Inline field validation for user input errors.
- Global error banner only for server/system failures.
- Navigation links for all major actions.
- Animated landing home page at `/` with role-based access cards.

### Reliability
- API returns consistent JSON response shapes.
- The UI must not submit invalid data.
- Audit log captures all create/update/delete operations.

### Security (Current Baseline)
- Basic password comparison (plain text — demo only).
- Teacher-scoped student isolation via `X-TeacherId` header.
- Student-scoped access via `X-StudentToken` header.
- Admin login is separate from the teacher login flow.

---

## Technical Requirements

### Backend
- ASP.NET Web API 5.2 (.NET Framework 4.8), OWIN self-hosted on port `5000`
- SQL Server LocalDB (`(localdb)\MSSQLLocalDB`) — `TrackMyGrade` database
- Entity Framework 6.4 (Code-First, `ApplicationDbContext`)
- FluentValidation 11.8 for server-side model validation
- AutoMapper 10.1 for DTO ↔ Entity mapping
- ELMAH for error logging (`Handlers/` + `Logging/`)
- Swashbuckle 5.6 for Swagger/OpenAPI docs at `http://localhost:5000/swagger`

#### REST Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/teachers/register` | Register teacher |
| POST | `/api/teachers/login` | Teacher login |
| GET | `/api/teachers/{id}` | Get teacher profile |
| GET | `/api/students` | List students (X-TeacherId header) |
| GET | `/api/students/{id}` | Get student detail |
| POST | `/api/students` | Create student |
| PUT | `/api/students/{id}` | Update student |
| DELETE | `/api/students/{id}` | Delete student |
| POST | `/api/student-auth/login` | Student login |
| GET | `/api/student-auth/profile` | Get student profile (X-StudentToken header) |
| PUT | `/api/student-auth/submit-assessments` | Student submits own scores |

### Frontend
- Angular 18 (standalone components, no NgModule)
- TypeScript 5.2 with `moduleResolution: "bundler"`
- Angular Router with `CanActivateFn` guards for teachers, students, and admins
- Template-driven forms with inline validation
- Development server: `http://localhost:4200`
- Build output: `StudentApp/dist/StudentApp/browser/`

#### Routes
| Path | Component | Access |
|------|-----------|--------|
| `/` | `HomeComponent` | Public |
| `/login` | `TeacherLoginComponent` | Public |
| `/register` | `RegisterComponent` | Public |
| `/list` | `StudentListComponent` | Teacher guard |
| `/create` | `StudentFormComponent` | Teacher guard |
| `/edit/:id` | `StudentFormComponent` | Teacher guard |
| `/detail/:id` | `StudentDetailComponent` | Teacher guard |
| `/student-login` | `StudentLoginComponent` | Public |
| `/student-dashboard` | `StudentDashboardComponent` | Student guard |
| `/admin` | `AdminLoginComponent` | Public |
| `/admin-dashboard` | `AdminDashboardComponent` | Admin guard |

---

## User Flows

### Register and Login (Teacher)
1. Teacher navigates to `/register`, completes the form.
2. On success, redirected to `/login`.
3. Teacher logs in with email and password, token stored in `localStorage`.

### Home Page
1. Any user lands on `/` and sees the animated landing page.
2. Three role cards are displayed: Student Access, Teacher Portal, Administration.
3. User clicks the relevant card to navigate to their login.

### Create Student
1. Teacher navigates to Add Student.
2. Fills out student info, grade, assessments, and sets an initial password.
3. Inline validation errors are shown for invalid fields.
4. Submit creates the student (generates `StudentNumber`) and redirects to list.

### Edit / Delete Student
1. Teacher selects Edit — form loads existing values; saves redirect to detail.
2. Teacher clicks Delete — confirmation dialog — student removed from list.

### Student Login and Dashboard
1. Student navigates to `/student-login`.
2. Enters email and password set by teacher.
3. On success, redirected to `/student-dashboard` showing profile, scores, and performance.

### Admin Login and Dashboard
1. Admin navigates to `/admin`.
2. Enters admin credentials.
3. On success, redirected to `/admin-dashboard` with tabs: Teachers, Students, Audit Logs.

---

## Acceptance Criteria
- Registration requires all fields and redirects to login on success.
- Teacher login succeeds with correct credentials; invalid credentials show inline error.
- Student login succeeds and loads personal dashboard.
- Admin login grants access to admin dashboard.
- Student form cannot submit with empty or invalid field values.
- Inline validation displays for all required fields on blur/submit.
- Global error banner shows only unexpected server errors.
- Students list updates after create/edit/delete.
- Audit log records all data mutations.

---

## Constraints and Assumptions
- Data persists in SQL Server LocalDB between restarts.
- Authentication is minimal and for demo purposes only (plain text passwords).
- Teacher data isolation relies on the `X-TeacherId` header (not a verified token).

---

## Risks
- Plain-text password storage is not production safe.
- `X-TeacherId` header is not cryptographically verified — ownership enforcement is trust-based.
- No authorization enforced on admin endpoints beyond login state.

---

## Future Enhancements
- Implement bcrypt password hashing.
- Replace GUID tokens with signed JWT with expiry and refresh.
- Role-based access control enforced server-side.
- Reporting and export (CSV/PDF).
- Class/subject taxonomy and analytics dashboards.
- Unit and integration test suites.
- CI/CD pipeline (GitHub Actions).
