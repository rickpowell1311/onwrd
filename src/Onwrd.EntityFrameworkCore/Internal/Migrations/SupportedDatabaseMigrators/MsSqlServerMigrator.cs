using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal class MsSqlServerMigrator
    {
        public async Task Migrate(DbConnection connection)
        {
            await CreateSchema(connection);
            await CreateEventsTable(connection);
        }

        private static async Task CreateSchema(DbConnection connection)
        {
            var createSchema = $@"
                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Onwrd')
                BEGIN
                    EXEC('CREATE SCHEMA [Onwrd]');
                END";

            var command = connection.CreateCommand();
            command.CommandText = createSchema;
            await command.ExecuteNonQueryAsync();
        }

        private static async Task CreateEventsTable(DbConnection connection)
        {
            var createEventsTable = $@"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Events')
                BEGIN
                    CREATE TABLE Onwrd.Events
                    (
	                    Id UNIQUEIDENTIFIER NOT NULL,
	                    CreatedOn datetime2 NOT NULL,
	                    DispatchedOn datetime2 NULL,
	                    TypeId NVARCHAR(MAX) NOT NULL,
	                    Contents NVARCHAR(MAX) NOT NULL,
                        AssemblyName NVARCHAR(MAX) NOT NULL,
	                    CONSTRAINT PK_Onwrd_Events PRIMARY KEY CLUSTERED (Id),
                        INDEX IX_Onwrd_Events_DispatchedOn NONCLUSTERED(DispatchedOn)
                    )
                END";

            var command = connection.CreateCommand();
            command.CommandText = createEventsTable;
            await command.ExecuteNonQueryAsync();
        }
    }
}
