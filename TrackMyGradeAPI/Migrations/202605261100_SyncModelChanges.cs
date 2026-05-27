namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Syncs the database schema with model changes defined in ApplicationDbContext,
    /// including Soft Delete columns, Timestamps, and Performance Indexes.
    /// </summary>
    public partial class SyncModelChanges : DbMigration
    {
        /// <summary>
        /// Applies the migration, adding new columns and creating indexes.
        /// </summary>
        public override void Up()
        {
            // 1. Add Tracking and Soft Delete columns (providing defaults for existing data)
            AddColumn("dbo.Subjects", "UpdatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.Subjects", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));

            AddColumn("dbo.ClassGroups", "UpdatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.ClassGroups", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));

            AddColumn("dbo.StudentEnrollments", "UpdatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.StudentEnrollments", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));

            AddColumn("dbo.Assignments", "UpdatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.Assignments", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));

            AddColumn("dbo.AssignmentSubmissions", "UpdatedAt", c => c.DateTime(nullable: false, defaultValueSql: "GETUTCDATE()"));
            AddColumn("dbo.AssignmentSubmissions", "IsDeleted", c => c.Boolean(nullable: false, defaultValue: false));

            // 2. Create Unique and Composite Indexes defined in OnModelCreating
            CreateIndex("dbo.Admins", "Email", unique: true, name: "IX_Admins_Email");
            CreateIndex("dbo.AuditLogs", "PerformedAt", name: "IX_AuditLogs_PerformedAt");
            CreateIndex("dbo.AuditLogs", new[] { "EntityType", "EntityId" }, name: "IX_AuditLogs_EntityType_EntityId");
            CreateIndex("dbo.ClassGroups", new[] { "SubjectId", "Name" }, unique: true, name: "IX_ClassGroups_SubjectId_Name");
            CreateIndex("dbo.Students", "Email", unique: true, name: "IX_Students_Email");
            CreateIndex("dbo.Students", "OmangOrPassport", unique: true, name: "IX_Students_OmangOrPassport");
            CreateIndex("dbo.Students", "StudentNumber", unique: true, name: "IX_Students_StudentNumber");
            CreateIndex("dbo.Subjects", "Code", unique: true, name: "IX_Subjects_Code");
            CreateIndex("dbo.Teachers", "Email", unique: true, name: "IX_Teachers_Email");
            CreateIndex("dbo.StudentEnrollments", new[] { "StudentId", "ClassGroupId" }, unique: true, name: "IX_StudentEnrollment_StudentId_ClassGroupId");
        }

        /// <summary>
        /// Reverts the migration, dropping the added columns and indexes.
        /// </summary>
        public override void Down()
        {
            DropIndex("dbo.StudentEnrollments", "IX_StudentEnrollment_StudentId_ClassGroupId");
            DropIndex("dbo.Teachers", "IX_Teachers_Email");
            DropIndex("dbo.Subjects", "IX_Subjects_Code");
            DropIndex("dbo.Students", "IX_Students_StudentNumber");
            DropIndex("dbo.Students", "IX_Students_OmangOrPassport");
            DropIndex("dbo.Students", "IX_Students_Email");
            DropIndex("dbo.ClassGroups", "IX_ClassGroups_SubjectId_Name");
            DropIndex("dbo.AuditLogs", "IX_AuditLogs_EntityType_EntityId");
            DropIndex("dbo.AuditLogs", "IX_AuditLogs_PerformedAt");
            DropIndex("dbo.Admins", "IX_Admins_Email");

            DropColumn("dbo.AssignmentSubmissions", "IsDeleted");
            DropColumn("dbo.AssignmentSubmissions", "UpdatedAt");
            DropColumn("dbo.Assignments", "IsDeleted");
            DropColumn("dbo.Assignments", "UpdatedAt");
            DropColumn("dbo.StudentEnrollments", "IsDeleted");
            DropColumn("dbo.StudentEnrollments", "UpdatedAt");
            DropColumn("dbo.ClassGroups", "IsDeleted");
            DropColumn("dbo.ClassGroups", "UpdatedAt");
            DropColumn("dbo.Subjects", "IsDeleted");
            DropColumn("dbo.Subjects", "UpdatedAt");
        }
    }
}