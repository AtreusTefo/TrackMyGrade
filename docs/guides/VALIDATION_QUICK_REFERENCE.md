# Admin Dashboard Validation - Quick Reference Guide

**Last Updated:** 2026-06-02

---

## 🎯 What Was Fixed

### Issue #1: No Real-Time Validation
**Before:** Admin had to click Submit to see errors  
**After:** Errors show as they type, invalid characters blocked

### Issue #2: Inconsistent Name Constraints
**Before:** Teachers could use 2-20 chars, Students could use 2-100 chars  
**After:** All use 2-100 chars consistently

### Issue #3: No Input Filtering
**Before:** Admin could paste special chars, numbers in name fields  
**After:** Only valid characters accepted (letters, spaces, hyphens, apostrophes)

---

## 📋 Validation Rules Summary

| Field | Type | Validation | Frontend | Backend |
|-------|------|-----------|----------|---------|
| **First Name** | Text | 2-100 letters, spaces, hyphens, apostrophes | Real-time filter | Regex match |
| **Last Name** | Text | 2-100 letters, spaces, hyphens, apostrophes | Real-time filter | Regex match |
| **Email** | Email | Valid email format, unique (case-insensitive) | Format check | Email validator + unique check |
| **Phone** | Text | Exactly 8 digits | Max 8, digits only | Regex `^\d{8}$` |
| **OMANG/Passport** | Text | Exactly 9 alphanumeric chars | Max 9, alphanumeric | Regex `^[a-zA-Z0-9]{9}$` |
| **Grade** | Select | 1-12 | Dropdown | Range check |
| **Teacher** | Select | Must select one | Required | FK exists check |
| **Subject** | Select | Must select one | Required | Required check |

---

## 🔧 How It Works

### Layer 1: Frontend (Angular Component)
```
User types → Input event → Handler filters input → Error message shows
	 ↓
onTeacherFirstNameInput("abc123") → removes "123" → stores "abc"
```

### Layer 2: Backend (C# Validator)
```
API receives data → AdminValidator.ValidateCreateTeacher() → Throws if invalid
	 ↓
Regex + length checks → Clear error message to frontend
```

### Layer 3: Database (EF6 + SQL Server)
```
Data saved → Unique constraints + FK checks → Prevents duplicates/orphans
```

---

## 🛠️ For Frontend Developers

### Adding Event to Form Field
```typescript
// Handler signature
onFieldNameInput(value: string): void {
  // Filter input
  const filtered = value.replace(/[PATTERN]/g, '');

  // Store filtered value
  this.model.fieldName = filtered;

  // Show error if invalid
  this.errors['fieldName'] = this.validateField(filtered);
}
```

### HTML Binding
```html
<input type="text" 
	   [(ngModel)]="model.fieldName"
	   (input)="onFieldNameInput($event.target.value)"
	   [class.input-error]="errors['fieldName']" />
<span *ngIf="errors['fieldName']" class="field-error">
  {{ errors['fieldName'] }}
</span>
```

### Character Filters
```typescript
// Letters only
value.replace(/[^a-zA-Z\s\-']/g, '')

// Digits only
value.replace(/\D/g, '')

// Alphanumeric only
value.replace(/[^a-zA-Z0-9]/g, '')
```

---

## 🛠️ For Backend Developers

### Using AdminValidator
```csharp
public AdminTeacherDto CreateTeacher(AdminCreateTeacherDto request)
{
	// ← This validates everything
	AdminValidator.ValidateCreateTeacher(request);

	// If we reach here, data is valid
	var teacher = new Teacher { ... };
	_db.Teachers.Add(teacher);
	_db.SaveChanges();

	// Log what happened
	_auditLogService.LogCreate("Teacher", teacher.Id, 
		new { teacher.FirstName, teacher.LastName, teacher.Email }, 
		"admin@trackmygrade.com");
}
```

### Custom Validation
```csharp
// Create custom rule if needed
if (request.TeacherId <= 0)
	throw new ArgumentException("Valid teacher must be selected.");

// Always check FKs exist before insert
if (!_db.Teachers.Any(t => t.Id == request.TeacherId))
	throw new KeyNotFoundException($"Teacher with ID {request.TeacherId} not found.");

// Check uniqueness at DB layer
if (_db.Students.Any(s => s.Email == request.Email.ToLower()))
	throw new InvalidOperationException("Email already exists.");
```

---

## 📱 For QA/Testers

### Test Cases

#### ✅ Valid Inputs (Should Save)
- First Name: "John" ✅
- First Name: "Mary Jane" ✅ (space)
- First Name: "Jean-Paul" ✅ (hyphen)
- First Name: "O'Brien" ✅ (apostrophe)
- Phone: "71234567" ✅ (8 digits)
- OMANG: "A1B2C3D4E" ✅ (9 alphanumeric)

#### ❌ Invalid Inputs (Should Block)
- First Name: "John123" ❌ (contains numbers)
- First Name: "John!" ❌ (special char)
- Phone: "712345" ❌ (only 6 digits)
- Phone: "7123456789" ❌ (10 digits, too many)
- OMANG: "A1B2C3D4" ❌ (only 8 chars)
- OMANG: "A1B2C3D4E5" ❌ (10 chars, too many)
- Email: "notanemail" ❌ (missing @)

#### 🔄 Behavior Tests
1. **Real-Time Filtering**
   - Type "abc123" in First Name field
   - Expected: "123" removed, only "abc" shown
   - Expected: Error message removed (valid input now)

2. **Error Message Display**
   - Leave First Name empty, click Submit
   - Expected: "First name is required" shown in red under field
   - Expected: Submit button disabled

3. **Duplicate Prevention**
   - Create teacher with email "john@example.com"
   - Try to create second teacher with same email
   - Expected: Error "A teacher with this email already exists"

---

## 🐛 Troubleshooting

### Issue: Field shows error but should be valid

**Check:**
1. Are you typing into the correct field? (phone vs name)
2. Is the error message specific? ("8 digits" vs "invalid")
3. Run backend validation: `AdminValidator.ValidateCreateTeacher(request)`

**Common Cause:** Space characters at end
- Frontend trim: `value.trim()`
- Backend trim: `request.FirstName.Trim()`

---

### Issue: Data saved but shouldn't have

**Check:**
1. Frontend validation ran? (error message showed)
2. Backend validator called? (check breakpoint in AdminValidator)
3. Database constraints exist? (FK, unique index)

**Debug:** Add breakpoint in AdminService.CreateTeacher()

---

### Issue: TypeScript compilation errors

**Error:** "Cannot find module '@angular/core'"  
**Cause:** Node modules not installed  
**Fix:** `cd StudentApp && npm install`

---

## 📚 File Locations

### Validators
- **C#:** `TrackMyGradeAPI/Application/Validators/AdminValidator.cs`
- **C#:** `TrackMyGradeAPI/Application/Validators/TeacherValidator.cs`
- **C#:** `TrackMyGradeAPI/Application/Validators/StudentValidator.cs`

### Services
- **C#:** `TrackMyGradeAPI/Application/Services/AdminService.cs`
- **TypeScript:** `StudentApp/src/app/services/admin-api.service.ts`

### Components
- **HTML:** `StudentApp/src/app/components/admin-dashboard/admin-dashboard.component.html`
- **TypeScript:** `StudentApp/src/app/components/admin-dashboard/admin-dashboard.component.ts`

### DTOs
- **C#:** `TrackMyGradeAPI/Application/DTOs/AdminDto.cs`
- **C#:** `TrackMyGradeAPI/Application/DTOs/StudentDto.cs`
- **C#:** `TrackMyGradeAPI/Application/DTOs/TeacherDto.cs`
- **TypeScript:** `StudentApp/src/app/models/admin.models.ts`

---

## 🚀 Quick Start for New Features

### Want to add a new admin form field?

1. **Add to TypeScript Model**
   ```typescript
   export interface CreateTeacherRequest {
	 firstName: string;
	 lastName: string;
	 email: string;
	 phone?: string;
	 subject: string;
	 department?: string;  // ← New field
   }
   ```

2. **Add to Admin Component**
   ```typescript
   onTeacherDepartmentInput(value: string): void {
	 this.newTeacher.department = value.trim();
	 this.teacherErrors['department'] = this.validateDepartment(value);
   }
   ```

3. **Add to HTML Form**
   ```html
   <div class="field-row">
	 <label for="teacher-dept">Department</label>
	 <input id="teacher-dept" type="text" 
			[(ngModel)]="newTeacher.department"
			(input)="onTeacherDepartmentInput($event.target.value)" />
   </div>
   ```

4. **Add to C# DTO**
   ```csharp
   public class AdminCreateTeacherDto {
	 public string FirstName { get; set; }
	 public string LastName { get; set; }
	 public string Email { get; set; }
	 public string Phone { get; set; }
	 public string Subject { get; set; }
	 public string Department { get; set; }  // ← New field
   }
   ```

5. **Add to C# Validator**
   ```csharp
   RuleFor(x => x.Department)
	 .NotEmpty().WithMessage("Department is required")
	 .MaximumLength(100).WithMessage("Department cannot exceed 100 chars");
   ```

---

## 📞 Questions?

Check the comprehensive documentation:
- `docs/implementation/VALIDATION_FIXES_SUMMARY.md` - Full technical details
- `docs/implementation/MANY_TO_MANY_ROADMAP.md` - Future enhancements
- `docs/ARCHITECTURE.md` - System design overview

