namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to fix FOREIGN KEY constraint name on ClassGroups table.
    /// Root cause: Database had a constraint named FK_dbo.ClassGroups_dbo.Subjects_CourseId
    /// which creates ambiguity because the constraint name doesn't match the actual column being referenced.
    /// The column is CourseId (legacy naming from Courses → Subjects rename) but references Subjects table.
    /// 
    /// This causes SqlException: "Either the parameter @objname is ambiguous or the claimed @objtype (OBJECT) is wrong"
    /// when Entity Framework tries to manage the schema during API initialization.
    /// </summary>
    public partial class FixSubjectsConstraintName : DbMigration
    {
        /// <summary>
        /// Applies the migration: renames the foreign key constraint to proper naming convention.
        /// </summary>
        public override void Up()
        {
            Sql(@"
-- Fix: Drop and recreate the FOREIGN KEY constraint on ClassGroups table
-- Problem: FK name was FK_dbo.ClassGroups_dbo.Subjects_CourseId (misleading)
-- Solution: Rename to FK_dbo.ClassGroups_dbo.Subjects_SubjectId (accurate)
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId' 
    AND OBJECT_NAME(parent_object_id) = 'ClassGroups'
)
BEGIN
    ALTER TABLE [dbo].[ClassGroups] 
    DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_CourseId];
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'
)
BEGIN
    ALTER TABLE [dbo].[ClassGroups] 
    ADD CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_SubjectId] 
    FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Subjects]([Id]);
END
");
        }

        /// <summary>
        /// Reverts the migration: restores the original constraint name.
        /// NOTE: Down migrations rarely execute in practice. This is included for completeness only.
        /// </summary>
        public override void Down()
        {
            Sql(@"
-- Revert: Restore old foreign key constraint name
IF EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_SubjectId'
)
BEGIN
    ALTER TABLE [dbo].[ClassGroups] 
    DROP CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_SubjectId];
END

IF NOT EXISTS (
    SELECT 1 FROM sys.foreign_keys 
    WHERE name = 'FK_dbo.ClassGroups_dbo.Subjects_CourseId'
)
BEGIN
    ALTER TABLE [dbo].[ClassGroups] 
    ADD CONSTRAINT [FK_dbo.ClassGroups_dbo.Subjects_CourseId] 
    FOREIGN KEY ([CourseId]) REFERENCES [dbo].[Subjects]([Id]);
END
");
        }
    }
}
