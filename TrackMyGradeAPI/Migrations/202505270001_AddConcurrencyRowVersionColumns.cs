namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to add optimistic concurrency control via RowVersion (Timestamp) columns.
    /// 
    /// Adds [RowVersion] SQL Server TIMESTAMP columns to critical grading entities:
    /// - StudentEnrollment: Prevents concurrent enrollment modifications
    /// - Assignment: Prevents concurrent assignment edits
    /// - AssignmentSubmission: Prevents concurrent grading conflicts
    /// 
    /// TIMESTAMP columns automatically increment on every UPDATE, enabling
    /// EF6's optimistic concurrency detection via DbUpdateConcurrencyException.
    /// </summary>
    public partial class AddConcurrencyRowVersionColumns : DbMigration
    {
        public override void Up()
        {
            // Add RowVersion to StudentEnrollment
            Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'StudentEnrollments' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[StudentEnrollments]
                    ADD [RowVersion] TIMESTAMP NULL;
                END
            ");

            // Add RowVersion to Assignment
            Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Assignments' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[Assignments]
                    ADD [RowVersion] TIMESTAMP NULL;
                END
            ");

            // Add RowVersion to AssignmentSubmission
            Sql(@"
                IF NOT EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AssignmentSubmissions' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[AssignmentSubmissions]
                    ADD [RowVersion] TIMESTAMP NULL;
                END
            ");
        }

        public override void Down()
        {
            // Revert by dropping RowVersion columns
            Sql(@"
                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'StudentEnrollments' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[StudentEnrollments]
                    DROP COLUMN [RowVersion];
                END

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'Assignments' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[Assignments]
                    DROP COLUMN [RowVersion];
                END

                IF EXISTS (
                    SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS 
                    WHERE TABLE_NAME = 'AssignmentSubmissions' AND COLUMN_NAME = 'RowVersion'
                )
                BEGIN
                    ALTER TABLE [dbo].[AssignmentSubmissions]
                    DROP COLUMN [RowVersion];
                END
            ");
        }
    }
}
