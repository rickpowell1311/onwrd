using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal class PostgreSqlMigrator
    {
        public async Task Migrate(DbConnection connection)
        {
            await CreateSchema(connection);
            await CreateEventsTable(connection);
        }

        private static async Task CreateSchema(DbConnection connection)
        {
            var createSchema = "CREATE SCHEMA IF NOT EXISTS \"Onwrd\";";

            var command = connection.CreateCommand();
            command.CommandText = createSchema;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateEventsTable(DbConnection connection)
        {
            var createEventsTable = @"
                CREATE TABLE IF NOT EXISTS ""Onwrd"".""Events""
                (
	                ""Id"" UUID NOT NULL,
	                ""CreatedOn"" timestamp(6) NOT NULL,
	                ""DispatchedOn"" timestamp(6) NULL,
	                ""TypeId"" TEXT NOT NULL,
	                ""Contents"" TEXT NOT NULL,
	                ""AssemblyName"" TEXT NOT NULL,
	                CONSTRAINT ""PK_Onwrd_Events"" PRIMARY KEY (""Id"")
                );

                CREATE INDEX IF NOT EXISTS ""IX_Onwrd_Events_DispatchedOn"" ON ""Onwrd"".""Events"" USING btree (""DispatchedOn"");
                END";

            var command = connection.CreateCommand();
            command.CommandText = createEventsTable;
            await command.ExecuteNonQueryAsync();
        }
    }
}
