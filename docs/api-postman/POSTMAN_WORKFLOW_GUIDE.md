# 🚀 POSTMAN TESTING WORKFLOW - QUICK GUIDE

## ⚠️ IMPORTANT: Request Order Matters!

You **MUST** follow this sequence for the API to work correctly:

```
Step 1: Register Teacher
   ↓
Step 2: Create Student (REQUIRED before Update!)
   ↓
Step 3: Update/Get/Delete Student
```

---

## 📋 Complete Testing Workflow

### Phase 1: Teacher Setup

#### 1️⃣ Register Teacher
```
Request: Teachers → Register Teacher
Method: POST /api/teachers/register
Expected: 200 OK

✅ What Happens:
- Teacher account created
- teacherId saved to environment (e.g., "1")
- authToken saved to environment
```

**Response Example:**
```json
{
  "id": 1,
  "firstName": "John",
  "lastName": "Doe",
  "email": "john.doe@school.com",
  "phone": "12345678",
  "subject": "Mathematics",
  "token": "guid-here"
}
```

---

### Phase 2: Student Creation (REQUIRED!)

#### 2️⃣ Create Student
```
Request: Students → Create Student
Method: POST /api/students
Expected: 201 Created

✅ What Happens:
- Student record created
- lastStudentId saved to environment (e.g., "1")
- Grades automatically calculated
```

**Response Example:**
```json
{
  "id": 1,  ← Saved as lastStudentId!
  "firstName": "Alice",
  "lastName": "Smith",
  "email": "alice.smith@student.com",
  "phone": "98765432",
  "grade": 10,
  "assessment1": 18,
  "assessment2": 16,
  "assessment3": 19,
  "total": 53,
  "average": 17.67,
  "percentage": 88.33,
  "performanceLevel": "Excellent"
}
```

**⚠️ CRITICAL:** 
- This step sets the `lastStudentId` variable
- Without this, Update/Get/Delete Student will fail with 404!

---

### Phase 3: Student Operations

#### 3️⃣ Get All Students
```
Request: Students → Get All Students
Method: GET /api/students
Expected: 200 OK

✅ What Happens:
- Returns all students for the teacher
- Verify your created student is in the list
```

#### 4️⃣ Get Student by ID
```
Request: Students → Get Student by ID
Method: GET /api/students/{{lastStudentId}}
Expected: 200 OK

✅ What Happens:
- Returns specific student details
- Uses lastStudentId from environment
```

#### 5️⃣ Update Student
```
Request: Students → Update Student
Method: PUT /api/students/{{lastStudentId}}
Expected: 200 OK

✅ What Happens:
- Updates student information
- Recalculates grades automatically
- Uses lastStudentId from environment
```

**⚠️ REQUIRES:** Step 2 (Create Student) must be run first!

#### 6️⃣ Delete Student
```
Request: Students → Delete Student
Method: DELETE /api/students/{{lastStudentId}}
Expected: 200 OK

✅ What Happens:
- Deletes the student
- Clears lastStudentId from environment
```

---

## ❌ Common Errors & Solutions

### Error: 404 - "{{lastStudentId}}" in URL

**Problem:**
```
URL: /api/students/{{lastStudentId}}
Error: 404 Not Found
Reason: Variable not replaced
```

**Solution:**
```
✅ Run "Create Student" first!
This sets the lastStudentId variable.
```

### Error: 400 - Phone validation

**Problem:**
```
Error: "Phone must be exactly 8 digits"
```

**Solution:**
```
✅ Use 8-digit phone numbers:
Valid: "12345678"
Invalid: "1234567890"
```

### Error: 400 - Assessment validation

**Problem:**
```
Error: "Assessment must be between 0 and 20"
```

**Solution:**
```
✅ Use scores 0-20:
Valid: 18, 16, 19
Invalid: 25, -5, 100
```

### Error: Environment not selected

**Problem:**
```
Variables not working
{{baseUrl}} not replaced
```

**Solution:**
```
✅ Select environment:
Top right dropdown → "TrackMyGrade - Local"
```

---

## 🔍 How to Check Variables

### View Environment Variables

1. Click **Environments** icon (left sidebar)
2. Select **"TrackMyGrade - Local"**
3. Check **Current Value** column

**Expected After Each Step:**

| After Step | Variable | Value | Status |
|------------|----------|-------|--------|
| Register Teacher | `teacherId` | `1` | ✅ |
| Register Teacher | `authToken` | `guid-string` | ✅ |
| Create Student | `lastStudentId` | `1` | ✅ |

### Quick Check with Eye Icon

1. Click **environment dropdown** (top right)
2. Click **eye icon (👁️)**
3. Verify all variables have values

---

## 📝 Request Order Reference

### ✅ CORRECT Order

```
1. Register Teacher
2. Create Student      ← Sets lastStudentId
3. Get All Students
4. Get Student by ID   ← Uses lastStudentId
5. Update Student      ← Uses lastStudentId
6. Delete Student      ← Uses lastStudentId
```

### ❌ WRONG Order (Will Fail!)

```
1. Register Teacher
2. Update Student      ← FAILS! No lastStudentId yet
```

**Error:** 404 Not Found - `{{lastStudentId}}` not replaced

---

## 🎯 Testing Checklist

### Pre-Testing
- [ ] API is running (port 5001)
- [ ] Swagger loads: http://localhost:5001/swagger
- [ ] Postman collection imported
- [ ] Environment "TrackMyGrade - Local" selected

### Test Execution
- [ ] Register Teacher (200 OK)
- [ ] Check: teacherId variable set
- [ ] Create Student (201 Created)
- [ ] Check: lastStudentId variable set
- [ ] Get All Students (200 OK)
- [ ] Get Student by ID (200 OK)
- [ ] Update Student (200 OK)
- [ ] Delete Student (200 OK)

### Validation
- [ ] All requests return expected status codes
- [ ] Environment variables populated correctly
- [ ] Student grades calculated automatically
- [ ] Phone numbers are 8 digits
- [ ] Assessment scores are 0-20

---

## 🆘 Troubleshooting

### Issue: Update Student returns 404

**Diagnosis:**
```powershell
# Check if lastStudentId is set
1. Open Postman
2. Click environment eye icon
3. Look for: lastStudentId
```

**If Empty:**
```
Solution: Run "Create Student" first
This will set the variable automatically
```

**If Has Value:**
```
Solution: Student might have been deleted
Run "Create Student" again to create a new one
```

### Issue: Variable shows {{lastStudentId}}

**This means Postman is NOT replacing the variable.**

**Causes:**
1. ❌ Environment not selected
2. ❌ Variable not set (student not created)
3. ❌ Wrong environment selected

**Solutions:**
1. ✅ Select "TrackMyGrade - Local" environment
2. ✅ Run "Create Student" to set the variable
3. ✅ Verify variable in environment editor

---

## 💡 Pro Tips

### Tip 1: Use Postman Console
```
View → Show Postman Console
See real-time logs of:
- Variables being set
- Request URLs (with variables replaced)
- Response data
```

### Tip 2: Collection Runner
```
Click "Run" on collection to test all requests in sequence
Automatically runs: Register → Create → Get → Update → Delete
```

### Tip 3: Save Responses
```
Click "Save Response" to keep examples
Useful for documentation and debugging
```

### Tip 4: Duplicate Requests
```
Right-click request → Duplicate
Test different scenarios without modifying original
```

---

## 📊 Variable Flow Diagram

```
┌──────────────────────────┐
│  Register Teacher        │
│  POST /teachers/register │
└────────────┬─────────────┘
             │
             ├─→ teacherId = 1
             ├─→ authToken = "guid"
             │
             ↓
┌──────────────────────────┐
│  Create Student          │
│  POST /students          │
│  Header: X-TeacherId: 1  │
└────────────┬─────────────┘
             │
             ├─→ lastStudentId = 1  ← IMPORTANT!
             │
             ↓
┌──────────────────────────┐
│  Update Student          │
│  PUT /students/1         │
│  (Uses lastStudentId)    │
└────────────┬─────────────┘
             │
             ├─→ Student updated ✅
             │
             ↓
┌──────────────────────────┐
│  Delete Student          │
│  DELETE /students/1      │
└────────────┬─────────────┘
             │
             ├─→ lastStudentId cleared
             │
             ↓
           Done!
```

---

## ✅ Success Criteria

**After following this workflow, you should see:**

✅ All requests return expected status codes  
✅ No `{{variable}}` syntax in URLs  
✅ Environment variables populated  
✅ Students created, updated, and deleted successfully  
✅ Automatic grade calculations working  
✅ No validation errors  

---

## 🎉 Summary

**Key Points:**
1. **Always create before you update!**
2. **Check environment variables are set**
3. **Follow the request order**
4. **Use 8-digit phone numbers**
5. **Use assessment scores 0-20**

**The Golden Rule:**
> You cannot update what doesn't exist!
> Always run Create Student BEFORE Update Student.

---

**Happy Testing! 🚀**

For more details, see:
- `API_404_UPDATE_STUDENT_FIX.md` - Complete fix guide
- `POSTMAN_INTEGRATION_GUIDE.md` - Full Postman documentation
- `START_HERE.txt` - Quick start guide
