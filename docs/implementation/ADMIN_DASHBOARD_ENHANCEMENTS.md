# Implementation Report: Admin Dashboard Enhancements

## Scope of Work
- Improve referential integrity and data consistency.
- Add Subject dropdown for creating a Teacher.
- Add Country Code dropdown for Phone fields (Teachers & Students).
- Implement Bulk Import via CSV for Teachers, Students, Subjects, and Class Groups.
- Implement Delete functionality for Subjects and Class Groups.

## Backend Changes
- Added `DeleteSubject(int id)` to `AdminController`, `IAdminService`, and `AdminService`. 
  - Validates referential integrity: prevents deletion if any Class Group is assigned to the Subject.
- Added `DeleteClassGroup(int id)` to `AdminController`, `IAdminService`, and `AdminService`.
  - Validates referential integrity: prevents deletion if any active enrollments or assignments exist.
- Modified `IAdminService` interface to include the new methods.

## Frontend Changes
- **Teacher Form**: 
  - Converted the `subject` text input to a `<select>` dropdown populated dynamically from the `subjects` array.
  - Replaced the standard phone text input with a combined Country Code `<select>` and phone `<input>` field. These are merged into `newTeacher.phone` on submission.
- **Student Form**:
  - Applied the same Country Code + Phone field pattern.
- **Bulk Import**:
  - Implemented `handleBulkImport` and `processBulkCsv` in `admin-dashboard.component.ts`.
  - Added hidden file inputs and UI buttons for importing Teachers, Students, Subjects, and Class Groups via CSV files.
  - The import process parses the CSV client-side and sequentially sends API requests, handling the UI state (`bulkUploading`) and displaying success/failure metrics afterward.
- **Delete Actions**:
  - Added 'Delete' buttons on the Subjects and Class Groups lists.
  - Bound these to `deleteSubject` and `deleteClassGroup` methods, complete with confirmation dialogs and error handling.

## Files Modified
1. `TrackMyGradeAPI/Application/Services/IAdminService.cs`
2. `TrackMyGradeAPI/Application/Services/AdminService.cs`
3. `TrackMyGradeAPI/Presentation/Controllers/AdminController.cs`
4. `StudentApp/src/app/services/admin-api.service.ts`
5. `StudentApp/src/app/components/admin-dashboard/admin-dashboard.component.ts`
6. `StudentApp/src/app/components/admin-dashboard/admin-dashboard.component.html`
