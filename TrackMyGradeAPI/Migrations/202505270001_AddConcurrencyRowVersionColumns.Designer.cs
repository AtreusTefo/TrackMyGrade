namespace TrackMyGradeAPI.Migrations
{
    using System;
    using System.Data.Entity.Migrations.Infrastructure;
    using System.Resources;

    public sealed partial class AddConcurrencyRowVersionColumns : IMigrationMetadata
    {
        private readonly ResourceManager Resources = new ResourceManager(typeof(AddConcurrencyRowVersionColumns));

        string IMigrationMetadata.Id
        {
            get { return "202505270001_AddConcurrencyRowVersionColumns"; }
        }

        string IMigrationMetadata.Source
        {
            get { return null; }
        }

        string IMigrationMetadata.Target
        {
            get { return Resources.GetString("Target"); }
        }
    }
}
