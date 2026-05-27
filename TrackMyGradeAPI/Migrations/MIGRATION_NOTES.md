Migrations Overview
===================

This document tracks all explicit migrations in the TrackMyGrade project and recent changes to the migration seeding process.

---

Migration 1: AddPhoneToAdmin
============================

Date: May 8, 2025

Purpose
-------
Adds a Phone column to the Admins table to support storing admin contact information and ensure frontend-backend data consistency.

Changes
-------
1. Adds Phone column to dbo.Admins table
   - Type: VARCHAR(20)
   - Nullable: NO
   - MaxLength: 20 characters

2. Updates default admin seed data to include phone number (71234567)

Reason for Change
-----------------
The frontend Admin interface expects a Phone field to be present in the admin object:
  export interface Admin {
    id: number;
    firstName: string;
    lastName: string;
    email: string;
    phone: string;
    token: string;
  }

Previously, the backend only returned Email and Token, causing a mismatch where the frontend would store an incomplete admin object. This led to issues with the admin dashboard authentication guard not functioning correctly.

Affected Files
--------------
- TrackMyGradeAPI/Models/Student.cs (Admin class)
- TrackMyGradeAPI/Application/DTOs/AdminDto.cs (AdminResponseDto)
- TrackMyGradeAPI/Application/Services/AdminService.cs (Login method)
- TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs (Admin entity mapping)
- TrackMyGradeAPI/Migrations/Configuration.cs (seed data)

---

Seeding Process Changes (May 25, 2025)
======================================

**Issue:** SqlException during database initialization - "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong"

**Root Cause:** The Seed() method was attempting to manage SQL check constraints through batch execution, which caused SQL Server ambiguity errors during migrations.

**Solution:** Simplified the Seed() method to focus only on essential data seeding:
- Removed the entire `EnsureDataIntegrityConstraints()` method
- Eliminated problematic SQL batch execution during migrations
- Data integrity is now enforced through:
  1. EF6 Foreign Keys (explicitly configured in OnModelCreating)
  2. FluentValidation in the Service layer
  3. Column constraints via fluent API

**Affected Files:**
- TrackMyGradeAPI/Migrations/Configuration.cs (simplified from ~120 lines to 57 lines)

**Why This Approach Is Better:**
1. No more SQL batch execution issues during startup
2. Validation logic is centralized in C# (Service layer)
3. Easier to debug and maintain
4. EF6 migrations handle all schema management

See: `docs/error-fixes/SQLEXCEPTION_AMBIGUOUS_OBJNAME_FIX.md` for full details.

---

How Migrations Work
-------------------
With AutomaticMigrationsEnabled = true in Configuration.cs, Entity Framework will:
1. Detect the schema changes when the application starts
2. Apply this migration automatically to the database
3. Create or update the database schema accordingly

Manual Application
------------------
If needed, you can apply migrations manually using Package Manager Console:
  Update-Database -TargetMigration AddPhoneToAdmin

Or via command line:
  cd TrackMyGradeAPI
  dotnet ef database update

Rollback
--------
To rollback this migration:
  Update-Database -TargetMigration 0
  (This removes all migrations and resets to initial state)

---

Data Integrity Strategy
=======================

Since check constraints are no longer created during seeding, data integrity is enforced through multiple layers:

1. **Frontend Validation** (Angular 18)
   - Reactive forms with validators matching backend rules
   - Client-side validation for UX feedback only

2. **Backend Service Layer Validation** (FluentValidation)
   - Phone format: 8 digits only
   - Email format: lowercase, valid email pattern
   - Grade range: 7-12 only
   - Scores and MaxScores: must be positive
   - All validators located in: TrackMyGradeAPI/Application/Validators/

3. **EF6 Fluent API Configuration** (ApplicationDbContext.cs)
   - Column constraints: NOT NULL, MaxLength, data types
   - Foreign key relationships with cascade rules
   - Unique indexes on Email, StudentNumber, OmangOrPassport

4. **Database Constraints** (SQL Server)
   - Foreign key relationships
   - Unique indexes
   - Column NOT NULL and type constraints

---

Best Practices for Future Migrations
====================================

1. **Use Code First Migrations for schema changes**
   - Always add/modify entity properties in models
   - Update OnModelCreating() fluent configuration
   - Entity Framework will detect and scaffold migrations

2. **Avoid custom SQL in Seed() method**
   - If SQL must execute, do so before migrations
   - Or create a separate initialization service
   - Never execute dynamic SQL batches during seeding

3. **Enforce validation in Service layer, not database**
   - Use FluentValidation for business rules
   - Database constraints should be minimal
   - This keeps validation logic in C# where it's testable

4. **Test migrations locally**
   - Always run and verify each migration locally
   - Check that the database schema is created correctly
   - Verify seed data is applied as expected

5. **Document migration purpose**
   - Every explicit migration should have a clear reason
   - Update this file with migration details
   - Link to related issues or requirements

---

Related Documentation
=====================

- `docs/error-fixes/SQLEXCEPTION_AMBIGUOUS_OBJNAME_FIX.md` - Full error analysis and fix details
- `TrackMyGradeAPI/Infrastructure/Data/ApplicationDbContext.cs` - EF6 fluent configuration
- `TrackMyGradeAPI/Application/Validators/` - FluentValidation rules
- `../.github/copilot-instructions.md` - Data Integrity Standards section
