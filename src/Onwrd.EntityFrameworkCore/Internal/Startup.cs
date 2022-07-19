using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.Migrations;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class Startup
    {
        private readonly OutboxingConfiguration outboxingConfiguration;
        private readonly MigrationContext migrationContext;

        public Startup(OutboxingConfiguration outboxingConfiguration, MigrationContext migrationContext)
        {
            this.outboxingConfiguration = outboxingConfiguration;
            this.migrationContext = migrationContext;
        }

        internal async Task InitializeAsync()
        {
            // Migrations
            if (this.outboxingConfiguration.RunMigrations && this.migrationContext.Database.IsRelational())
            {
                await migrationContext.Database.MigrateAsync();
            }
        }

        internal void Initialize()
        {
            // Migrations
            if (this.outboxingConfiguration.RunMigrations && this.migrationContext.Database.IsRelational())
            {
                migrationContext.Database.Migrate();
            }
        }
    }
}
