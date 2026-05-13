namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    /// <summary>
    /// Migration to add Phone column to the Admin table.
    /// Enables storing admin contact information and ensures frontend-backend data consistency.
    /// </summary>
    public partial class AddPhoneToAdmin : DbMigration
    {
        /// <summary>
        /// Applies the migration: adds Phone column to Admin table.
        /// </summary>
        public override void Up()
        {
            AddColumn("dbo.Admins", "Phone", c => c.String(maxLength: 20));
        }

        /// <summary>
        /// Reverts the migration: removes Phone column from Admin table.
        /// </summary>
        public override void Down()
        {
            DropColumn("dbo.Admins", "Phone");
        }
    }
}
