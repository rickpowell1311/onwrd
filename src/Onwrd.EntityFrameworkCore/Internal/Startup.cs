using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal.Migrations;
using Onwrd.EntityFrameworkCore.Internal.Migrations.Updates;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class Startup
    {
        private readonly OnwrdConfiguration onwrdConfiguration;
        private readonly MigrationServices migrationServices;

        public Startup(OnwrdConfiguration onwrdConfiguration, MigrationServices migrationServices)
        {
            this.onwrdConfiguration = onwrdConfiguration;
            this.migrationServices = migrationServices;
        }

        internal async Task InitializeAsync()
        {
            // Migrations
            if (this.onwrdConfiguration.RunMigrations)
            {
                using var scope = this.migrationServices.ServiceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetService<MigrationContext>();

                if (context.Database.IsRelational())
                {
                    await context.Database.MigrateAsync();
                }
            }
        }

        internal void Initialize()
        {
            // Migrations
            if (this.onwrdConfiguration.RunMigrations)
            {
                using var scope = this.migrationServices.ServiceProvider.CreateScope();
                using var context = scope.ServiceProvider.GetService<MigrationContext>();

                if (context.Database.IsRelational())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
