# Postman Integration Guide

## 📦 Overview

This project is now fully integrated with Postman for easy API testing. The Postman collection includes:

- ✅ All API endpoints (Teachers & Students)
- ✅ Automated test scripts for response validation
- ✅ Pre-request scripts for authentication
- ✅ Environment variables for easy configuration
- ✅ Test scenarios for validation testing

---

## 🚀 Quick Start

### 1. Import Postman Collection

1. Open **Postman**
2. Click **Import** (top left)
3. Select **TrackMyGradeAPI.postman_collection.json**
4. Click **Import**

### 2. Import Environment

1. Click the **Environments** icon (left sidebar)
2. Click **Import**
3. Select **TrackMyGradeAPI.postman_environment.json**
4. Click **Import**
5. Select **TrackMyGrade - Local** from the environment dropdown (top right)

### 3. Start the API

Run the API using one of these methods:

```powershell
# Option 1: Using PowerShell script
.\start-api.ps1

# Option 2: Direct execution
.\TrackMyGradeAPI.exe

# Option 3: Using dotnet (if in development)
dotnet run
```

The API will start on: **http://localhost:5000**

---

## 📋 Testing Workflow

### Step 1: Register a Teacher

1. Open the collection: **TrackMyGrade API**
2. Go to: **Teachers > Register Teacher**
3. Click **Send**

✅ The teacher ID and token will be **automatically saved** to your environment variables

### Step 2: Test Student Operations

Now you can test all student endpoints:

1. **Get All Students** - Returns all students for your teacher
2. **Create Student** - Creates a new student (ID auto-saved)
3. **Get Student by ID** - Retrieves the created student
4. **Update Student** - Updates the student's information
5. **Delete Student** - Removes the student

All requests automatically include the `X-TeacherId` header using the saved environment variable.

---

## 🔐 Authentication

The API uses a simple header-based authentication:

- **Header Name**: `X-TeacherId`
- **Header Value**: Teacher ID from registration/login

This is **automatically handled** by the Postman collection using environment variables.

---

## 📊 API Endpoints Reference

### Teachers

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/teachers/register` | Register a new teacher |
| POST | `/api/teachers/login` | Login and get auth token |
| GET | `/api/teachers/{id}` | Get teacher profile |

### Students

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/students` | Get all students for teacher |
| GET | `/api/students/{id}` | Get specific student |
| POST | `/api/students` | Create new student |
| PUT | `/api/students/{id}` | Update student |
| DELETE | `/api/students/{id}` | Delete student |

---

## ✅ Automated Tests

Each request includes automated tests that verify:

### Registration/Login
- ✅ Status code is 200
- ✅ Response contains ID and token
- ✅ Environment variables are set automatically

### Student Operations
- ✅ Status codes (200, 201, 400)
- ✅ Response structure validation
- ✅ Calculated fields (total, average, percentage, performanceLevel)
- ✅ Data integrity

### Validation Tests
- ✅ Invalid scores (> 20)
- ✅ Negative scores
- ✅ Missing required fields

---

## 📝 Sample Request Bodies

### Register Teacher
```json
{
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phone": "12345678",
  "subject": "Mathematics",
  "password": "password123"
}
```

**Note:** Phone must be exactly 8 digits.

### Create Student
```json
{
  "firstName": "Alice",
  "lastName": "Smith",
  "email": "alice.smith@student.com",
  "phone": "98765432",
  "grade": 10,
  "assessment1": 18,
  "assessment2": 16,
  "assessment3": 19
}
```

**Note**: Assessment scores must be between 0-20. The API automatically calculates:
- Total (sum of all assessments)
- Average (mean score)
- Percentage (total/60 * 100)
- Performance Level (Excellent, Good, Satisfactory, Needs Support)

---

## 🔍 Viewing Test Results

After sending a request:

1. Click the **Test Results** tab in the response section
2. View passed/failed tests
3. Check the **Console** (bottom) for detailed logs

---

## 🌐 Environment Variables

The collection uses these environment variables:

| Variable | Description | Auto-Set |
|----------|-------------|----------|
| `baseUrl` | API base URL (default: http://localhost:5000) | No |
| `teacherId` | Current teacher's ID | Yes |
| `authToken` | Current auth token | Yes |
| `lastStudentId` | Last created/accessed student ID | Yes |

---

## 🛠️ Troubleshooting

### API Not Responding
```powershell
# Check if API is running
Test-NetConnection localhost -Port 5000

# Restart the API
.\start-api.ps1
```

### Teacher ID Not Set
- Run **Register Teacher** or **Login Teacher** first
- The ID is automatically saved to environment variables

### Validation Errors
- Check that assessment scores are between 0-20
- Ensure all required fields are provided
- Verify email format is valid

### Database Issues
The database file is created automatically at:
```
TrackMyGrade.db (in the same folder as TrackMyGradeAPI.exe)
```

---

## 📚 Additional Resources

- **Swagger UI**: http://localhost:5000/swagger
- **API Documentation**: See XML comments in controller files
- **PowerShell Test Scripts**: 
  - `test-api.ps1` - Basic API tests
  - `start-api.ps1` - Start the API server

---

## 🎯 Best Practices

1. **Always start with teacher registration** before testing student endpoints
2. **Use the environment selector** to switch between configurations (local/staging/prod)
3. **Check test results** after each request to ensure validation
4. **Use the Test Scenarios** folder to verify error handling
5. **Monitor the Console** for detailed execution logs

---

## 💡 Tips

- **Ctrl+Enter** to send request quickly
- **Ctrl+/** to comment/uncomment in scripts
- Use **Collections Runner** to run all tests in sequence
- **Export environment** to share with team members
- Create additional environments for different servers (dev, staging, production)

---

## 🔄 Updating the Collection

If the API changes:

1. Update the collection JSON file
2. Re-import in Postman (overwrites existing)
3. Share updated files with team

---

## 📞 Support

For issues or questions:
1. Check TROUBLESHOOTING.md in the project root
2. Review API logs in the console output
3. Check ELMAH error logs (if configured)

---

**Happy Testing! 🚀**
