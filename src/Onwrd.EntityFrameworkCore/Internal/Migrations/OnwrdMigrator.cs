using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Storage;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal class OnwrdMigrator : Migrator
    {
        public OnwrdMigrator(
            IMigrationsAssembly migrationsAssembly, 
            IHistoryRepository historyRepository, 
            IDatabaseCreator databaseCreator, 
            IMigrationsSqlGenerator migrationsSqlGenerator, 
            IRawSqlCommandBuilder rawSqlCommandBuilder, 
            IMigrationCommandExecutor migrationCommandExecutor, 
            IRelationalConnection connection, 
            ISqlGenerationHelper sqlGenerationHelper, 
            ICurrentDbContext currentContext, 
            IModelRuntimeInitializer modelRuntimeInitializer, 
            IDiagnosticsLogger<DbLoggerCategory.Migrations> logger, 
            IRelationalCommandDiagnosticsLogger commandLogger, 
            IDatabaseProvider databaseProvider) : 
                base(
                    migrationsAssembly,
                    OverrideHistoryRepositorySchema(historyRepository, databaseProvider),
                    databaseCreator,
                    migrationsSqlGenerator,
                    rawSqlCommandBuilder,
                    migrationCommandExecutor,
                    connection,
                    sqlGenerationHelper,
                    currentContext,
                    modelRuntimeInitializer,
                    logger,
                    commandLogger,
                    databaseProvider)
        {
        }

        private static IHistoryRepository OverrideHistoryRepositorySchema(
            IHistoryRepository historyRepository,
            IDatabaseProvider databaseProvider)
        {
            var propertyName = "TableSchema";
            var readonlyBackingFieldName = $"<{propertyName}>k__BackingField";

            var tableSchemaField = typeof(HistoryRepository)
                .GetField(
                    readonlyBackingFieldName,
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

            if (tableSchemaField == null)
            {
                throw new Exception("Unable to configure EF core migrations HistoryRepository schema for Onwrd");
            }

            var schemaName = string.Empty;


            switch (databaseProvider.Name)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                    schemaName = "Onwrd";
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    schemaName = "onwrd";
                    break;
                throw new NotSupportedException($"Provider '{databaseProvider.Name}' is not supported");
            }

            tableSchemaField.SetValue(historyRepository, schemaName);

            return historyRepository;
        }
    }
}
