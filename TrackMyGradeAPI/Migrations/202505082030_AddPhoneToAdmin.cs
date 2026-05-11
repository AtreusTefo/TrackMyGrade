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
        public override void Up()
        {
            AddColumn("dbo.Admins", "Phone", c => c.String(maxLength: 20));
        }

        public override void Down()
        {
            DropColumn("dbo.Admins", "Phone");
        }
    }
}
