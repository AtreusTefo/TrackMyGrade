namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to fix FOREIGN KEY constraint name on ClassGroups table.
    /// 
    /// Root Cause: The constraint FK_dbo.ClassGroups_dbo.Subjects_CourseId references 
    /// the Subjects table via the CourseId column, but the constraint name contains 
    /// "CourseId" which creates ambiguity in SQL Server schema operations.
    /// 
    /// Fix: Rename constraint to FK_dbo.ClassGroups_dbo.Subjects_SubjectId to align 
    /// with Entity Framework conventions and eliminate ambiguity.
    /// </summary>
    public partial class FixSubjectsConstraintName : DbMigration
    {
        // Drop the ambiguous constraint and create a properly-named replacement
        public override void Up()
        {
            Sql(@"
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

        // Revert to original constraint name if migration is rolled back
        public override void Down()
        {
            Sql(@"
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