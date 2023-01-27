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

        public async Task Migrate(DbConnection connection)
        {
            switch (this.dbProvider)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    await new MsSqlServerMigrator().Migrate(connection);
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    await new PostgreSqlMigrator().Migrate(connection);
                    break;
                default:
                    throw new NotImplementedException("Database provider not supported");
            }
        }
    }
}
