namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to fix PRIMARY KEY and FOREIGN KEY constraint names on Subjects and ClassGroups tables.
    /// Root cause: Database had misnamed constraints from a previous Courses → Subjects table rename:
    /// - Subjects table: PK_dbo.Courses (should be PK_dbo.Subjects)
    /// - ClassGroups table: FK_dbo.ClassGroups_dbo.Courses_CourseId (should reference Subjects, not Courses)
    /// 
    /// This causes SqlException: "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong"
    /// when Entity Framework tries to manage the schema during API initialization.
    /// </summary>
    public partial class FixSubjectsConstraintName : DbMigration
    {
        /// <summary>
        /// Applies the migration: renames both constraints to match current table names.
        /// </summary>
        public override void Up()
        {
            Sql(@"
-- Fix 1: Drop and recreate the PRIMARY KEY constraint on Subjects table
IF EXISTS (SELECT 1 FROM sys.objects WHERE name = 'PK_dbo.Courses' AND type = 'PK' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[Subjects] DROP CONSTRAINT [PK_dbo.Courses];
END

IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = 'PK_dbo.Subjects' AND type = 'PK' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [PK_dbo.Subjects] PRIMARY KEY ([Id]);
END

-- Fix 2: Drop and recreate the FOREIGN KEY constraint on ClassGroups table
-- The old constraint references the obsolete table name 'Courses'
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_dbo.ClassGroups_dbo.Courses_CourseId' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[ClassGroups] DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Courses_CourseId];
END

-- Verify SubjectId column exists (it should, but handle gracefully if migration runs twice)
IF NOT EXISTS (
    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
    WHERE TABLE_NAME = 'ClassGroups' AND COLUMN_NAME = 'SubjectId' AND TABLE_SCHEMA = 'dbo'
)
BEGIN
    -- If SubjectId doesn't exist, something is very wrong; this prevents constraint creation from failing
    RAISERROR('ERROR: ClassGroups.SubjectId column not found. Database schema is corrupted.', 16, 1);
END

-- Recreate the foreign key with the correct constraint name referencing Subjects
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[ClassGroups] 
    ADD CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_SubjectId] 
    FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END
");
        }

        /// <summary>
        /// Reverts the migration: restores the original (incorrect) constraint names.
        /// NOTE: Down migrations rarely execute in practice. This is included for completeness only.
        /// </summary>
        public override void Down()
        {
            Sql(@"
-- Revert Fix 2: Restore old foreign key constraint name
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId' 
    AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    ALTER TABLE [dbo].[ClassGroups] DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_SubjectId];
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_dbo.ClassGroups_dbo.Courses_CourseId' AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[ClassGroups]
    ADD CONSTRAINT [FK_dbo.ClassGroups_dbo.Courses_CourseId]
    FOREIGN KEY ([SubjectId]) REFERENCES [dbo].[Subjects]([Id]);
END

-- Revert Fix 1: Restore old primary key constraint name
IF EXISTS (
    SELECT 1 FROM sys.objects 
    WHERE name = 'PK_dbo.Subjects' 
    AND type = 'PK' 
    AND schema_id = SCHEMA_ID('dbo')
)
BEGIN
    ALTER TABLE [dbo].[Subjects] DROP CONSTRAINT [PK_dbo.Subjects];
END

IF NOT EXISTS (
    SELECT 1 FROM sys.objects 
    WHERE name = 'PK_dbo.Courses' 
    AND type = 'PK' 
    AND schema_id = SCHEMA_ID('dbo'))
BEGIN
    ALTER TABLE [dbo].[Subjects] ADD CONSTRAINT [PK_dbo.Courses] PRIMARY KEY ([Id]);
END
");
        }
    }
}
