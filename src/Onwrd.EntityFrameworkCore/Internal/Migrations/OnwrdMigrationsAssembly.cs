using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using System.Reflection;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal class OnwrdMigrationsAssembly : MigrationsAssembly
    {
        private readonly ICurrentDbContext currentContext;

        public OnwrdMigrationsAssembly(
            ICurrentDbContext currentContext,
            IDbContextOptions options,
            IMigrationsIdGenerator idGenerator,
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger) : base(currentContext, options, idGenerator, logger)
        {
            this.currentContext = currentContext;
        }

        public override Migration CreateMigration(TypeInfo migrationClass, string activeProvider)
        {
            var migration = base.CreateMigration(migrationClass, activeProvider);
            
            if (migration is IOnwrdMigration onwrdMigration
                && this.currentContext.Context is MigrationContext migrationContext)
            {
                onwrdMigration.Context = migrationContext;
            }

            return migration;
        }
    }
}
