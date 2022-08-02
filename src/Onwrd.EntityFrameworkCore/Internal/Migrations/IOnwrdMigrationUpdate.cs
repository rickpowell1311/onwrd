using Microsoft.EntityFrameworkCore.Migrations;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal interface IOnwrdMigrationUpdate
    {
        string GetSqlForMsSqlServer();

        string GetSqlForPostgreSql();
    }

    internal static class OnwardMigrationUpdateExtensions
    {
        internal static void Sql<T>(
            this MigrationBuilder migrationBuilder)
                where T : class, IOnwrdMigrationUpdate, new()
        {
            var update = new T();

            switch (migrationBuilder.ActiveProvider)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    migrationBuilder.SqlIfNotNull(update.GetSqlForMsSqlServer());
                    return;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    migrationBuilder.SqlIfNotNull(update.GetSqlForPostgreSql());
                    return;
            }

            throw new NotSupportedException($"Provider '{migrationBuilder.ActiveProvider}' is not supported");
        }

        private static void SqlIfNotNull(
            this MigrationBuilder migrationBuilder,
            string sql)
        {
            if (string.IsNullOrWhiteSpace(sql))
            {
                return;
            }

            migrationBuilder.Sql(sql);
        }
    }
}
