using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal class SqlServerDatabaseMigrator : IDatabaseMigrator
    {
        public async Task MigrateAsync(DbConnection connection)
        {
            await connection.ExecuteSqlAsync(CreateSchemaSql());
            await connection.ExecuteSqlAsync(CreateEventsTableSql());
        }

        public void Migrate(DbConnection connection)
        {
            connection.ExecuteSql(CreateSchemaSql());
            connection.ExecuteSql(CreateEventsTableSql());
        }

        private static string CreateSchemaSql()
        {
            return $@"
                IF NOT EXISTS (SELECT * FROM sys.schemas WHERE name = 'Onwrd')
                BEGIN
                    EXEC('CREATE SCHEMA [Onwrd]');
                END";
        }

        private static string CreateEventsTableSql()
        {
            return $@"
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
        }
    }
}
