# TrackMyGrade - Quick Start Guide

Get the TrackMyGrade application running in 5 minutes!

## Prerequisites Checklist

- [ ] Visual Studio 2019+ (.NET Framework 4.8)
- [ ] Node.js 18+ and npm 9+
- [ ] Angular CLI: `npm install -g @angular/cli`
- [ ] Git (optional)

## Backend Setup (5 minutes)

### Step 1: Navigate to Backend
```bash
cd TrackMyGradeAPI
```

### Step 2: Open in Visual Studio
- Double-click `TrackMyGradeAPI.csproj` OR
- Open Visual Studio → File → Open Project

### Step 3: Restore NuGet Packages
```
Tools > NuGet Package Manager > Package Manager Console
Update-Package
```

### Step 4: Run Backend
```
Debug > Start Debugging (F5)
```

✅ **Backend running at:** `http://localhost:5000`

**Expected Output:**
```
Information: Application started. Press Ctrl+C to shut down.
Information: Hosting environment: Development
Information: Content root path: C:\...\TrackMyGradeAPI
```

## Frontend Setup (3 minutes)

### Step 1: Open New Terminal
```bash
cd StudentApp
```

### Step 2: Install Dependencies
```bash
npm install
```

### Step 3: Start Development Server
```bash
npm start
```

✅ **Frontend running at:** `http://localhost:4200`

**Expected Output:**
```
✔ Build successful
✔ Compiled successfully.

Application bundle generated successfully. 1.2 MB
```

## First Test Run (2 minutes)

### 1. Open Browser to Frontend
```
http://localhost:4200
```

### 2. Register a Teacher
- Click "Register here" link
- Fill in sample data:
  ```
  First Name: John
  Last Name: Doe
  Email: john@example.com
  Phone: 12345678
  Subject: Mathematics
  Password: password123
  Confirm: password123
  ```
- Click Register

### 3. Redirects to Login
- Enter registered email and password
- Click Login

### 4. Dashboard (Student List)
- Empty list shown (no students yet)
- Click "Add Student" button

### 5. Add a Student
- Fill in sample data:
  ```
  First Name: Jane
  Last Name: Smith
  Email: jane@example.com
  Phone: 87654321
  Grade: 9
  Assessment 1: 18
  Assessment 2: 19
  Assessment 3: 17
  ```
- See calculations auto-populate:
  ```
  Total: 54/60
  Average: 18.00
  Percentage: 90.00%
  Performance: Excellent
  ```
- Click Create

### 6. View Results
- Back to list showing Jane Smith
- Percentage: 90.00%
- Performance: Excellent (green badge)

### 7. View Details
- Click "View" button on student row
- See full student profile with detailed metrics

### 8. Edit Student
- Click "Edit" button
- Change Assessment 1 to 15
- Total changes to 51, Percentage: 85.00%
- Performance: Good (blue badge)
- Click Update

### 9. Delete Student
- Back to list
- Click "Delete" button
- Confirm deletion
- List becomes empty

### 10. Logout
- Click "Logout" button in navbar
- Redirected to Login page

## Stop Servers

### Stop Backend
```
Visual Studio: Debug > Stop Debugging (Shift+F5)
OR press Ctrl+C in command prompt
```

### Stop Frontend
```
Terminal: Ctrl+C
```

## Common Issues & Solutions

### Issue: Backend won't start
**Solution:**
```bash
# Port 5000 might be in use
netstat -ano | findstr :5000
taskkill /PID <PID> /F
# Then retry F5 in Visual Studio
```

### Issue: Frontend shows "Cannot GET /"
**Solution:**
```bash
cd StudentApp
npm install
npm start
```

### Issue: API call fails (CORS error)
**Solution:**
- Ensure backend is running: http://localhost:5000
- Check browser DevTools Network tab
- Backend should return JSON responses

### Issue: Login doesn't work
**Solution:**
1. Verify backend is running
2. Check email matches registered email exactly
3. Clear browser localStorage: DevTools → Application → LocalStorage → Clear All

### Issue: Calculations not showing
**Solution:**
- Ensure all assessment fields are filled with 0-20 values
- Change any assessment field to trigger calculations
- Refresh page if needed

## Development Workflow

### Making Changes to Backend
1. Edit code in Visual Studio
2. Quick Refresh (Ctrl+Shift+F5) to recompile
3. Or stop/start debugging

### Making Changes to Frontend
1. Edit `.ts` or `.html` files in StudentApp/src
2. Webpack automatically rebuilds
3. Browser auto-refreshes

### Adding New Features
1. Backend:
   - Add model/entity
   - Create DTO
   - Create validator
   - Create service method
   - Add controller endpoint
   
2. Frontend:
   - Add service method
   - Create/update component
   - Add routing if needed
   - Style new component

## File Structure for Reference

```
TrackMyGrade/
├── TrackMyGradeAPI/
│   ├── Models/
│   │   └── Student.cs (entities)
│   ├── DTOs/
│   │   └── Dtos.cs (transfer objects)
│   ├── Services/
│   │   ├── TeacherService.cs
│   │   └── StudentService.cs
│   ├── Controllers/
│   │   ├── TeachersController.cs
│   │   └── StudentsController.cs
│   ├── Validators/
│   │   └── DtoValidators.cs
│   └── WebApiConfig.cs (CORS setup)
│
├── StudentApp/
│   ├── src/app/
│   │   ├── components/
│   │   │   ├── login/
│   │   │   ├── register/
│   │   │   ├── student-list/
│   │   │   ├── student-form/
│   │   │   └── student-detail/
│   │   ├── services/
│   │   │   ├── auth.service.ts
│   │   │   └── student.service.ts
│   │   ├── models/
│   │   │   └── index.ts (interfaces)
│   │   └── app.routes.ts (routing)
│   └── package.json
│
├── README.md (detailed setup)
├── ARCHITECTURE.md (system design)
└── QUICK_START.md (this file)
```

## API Endpoints Quick Reference

```
Teacher Endpoints
POST   /api/teachers/register       Register new teacher
POST   /api/teachers/login          Login (returns token)
GET    /api/teachers/{id}           Get profile

Student Endpoints
GET    /api/students                List all (for teacher)
GET    /api/students/{id}           Get student details
POST   /api/students                Create student
PUT    /api/students/{id}           Update student
DELETE /api/students/{id}           Delete student
```

## Test Data

### Teacher Account
```
Email: john@example.com
Password: password123
Subject: Mathematics
```

### Sample Students (to create)
```
Student 1: Jane Smith
├─ Grade: 9
├─ Assessments: 18, 19, 17
├─ Total: 54/60
├─ Percentage: 90%
└─ Level: Excellent

Student 2: Bob Johnson
├─ Grade: 10
├─ Assessments: 12, 10, 11
├─ Total: 33/60
├─ Percentage: 55%
└─ Level: Satisfactory

Student 3: Alice Brown
├─ Grade: 8
├─ Assessments: 8, 9, 8
├─ Total: 25/60
├─ Percentage: 41.67%
└─ Level: Needs Support
```

## Next Steps

1. **Explore the code** - Review `ARCHITECTURE.md` for design details
2. **Try different scenarios** - Edit, delete, create multiple students
3. **Test validations** - Try invalid data to see error messages
4. **Check calculations** - Verify math for different assessment values
5. **Customize** - Change colors, add fields, extend functionality

## Performance Tips

- **Frontend**: Runs on port 4200, production build: `npm run build`
- **Backend**: Responds in <500ms per API call
- **Database**: In-memory, ultra-fast for development
- **Build time**: Frontend builds in ~10-15 seconds

## Need Help?

- Check `README.md` for detailed documentation
- Review `ARCHITECTURE.md` for system design
- Run with debugging enabled in Visual Studio
- Check browser DevTools Network tab for API issues
- Review browser Console for JavaScript errors

---

**Happy Testing!** 🎉

Once you're comfortable with the setup, explore the codebase to understand how everything works together.
