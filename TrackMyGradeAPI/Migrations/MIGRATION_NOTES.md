Migration: AddPhoneToAdmin
========================

Date: May 8, 2025

Purpose
-------
This migration adds a Phone column to the Admins table to support storing admin contact information and ensure frontend-backend data consistency.

Changes
-------
1. Adds Phone column to dbo.Admins table
   - Type: VARCHAR(20)
   - Nullable: YES
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
