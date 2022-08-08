using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.Migrations;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class Startup
    {
        private readonly OnwrdConfiguration onwrdConfiguration;
        private readonly MigrationContext migrationContext;

        public Startup(OnwrdConfiguration onwrdConfiguration, MigrationContext migrationContext)
        {
            this.onwrdConfiguration = onwrdConfiguration;
            this.migrationContext = migrationContext;
        }

        internal async Task InitializeAsync()
        {
            // Migrations
            if (this.onwrdConfiguration.RunMigrations && this.migrationContext.Database.IsRelational())
            {
                await migrationContext.Database.MigrateAsync();
            }
        }

        internal void Initialize()
        {
            // Migrations
            if (this.onwrdConfiguration.RunMigrations && this.migrationContext.Database.IsRelational())
            {
                migrationContext.Database.Migrate();
            }
        }
    }
}
