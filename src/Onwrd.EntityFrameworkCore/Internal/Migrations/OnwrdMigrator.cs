using Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators;
using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal class OnwrdMigrator : IOnwrdMigrator
    {
        private readonly string dbProvider;

        public OnwrdMigrator(string dbProvider)
        {
            this.dbProvider = dbProvider;
        }

        public async Task MigrateAsync(DbConnection connection)
        {
            switch (this.dbProvider)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    await MsSqlServerMigrator.MigrateAsync(connection);
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    await PostgreSqlMigrator.MigrateAsync(connection);
                    break;
                default:
                    throw new NotImplementedException("Database provider not supported");
            }
        }

        public void Migrate(DbConnection connection)
        {
            switch (this.dbProvider)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    MsSqlServerMigrator.Migrate(connection);
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    PostgreSqlMigrator.Migrate(connection);
                    break;
                default:
                    throw new NotImplementedException("Database provider not supported");
            }
        }
    }
}
