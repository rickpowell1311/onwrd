using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public static class SupportedDatabases
    {
        public static TheoryData<ISupportedDatabase> All
        {
            get
            {
                var theoryData = new TheoryData<ISupportedDatabase>();

                foreach (var supportedDatabaseType in typeof(SupportedDatabases)
                    .Assembly
                    .GetTypes()
                        .Where(x => x.IsClass && typeof(ISupportedDatabase).IsAssignableFrom(x)))
                {
                    var supportedDatabase = (ISupportedDatabase)Activator.CreateInstance(supportedDatabaseType);
                    theoryData.Add(supportedDatabase);
                }

                return theoryData;
            }
        }

        public class SqlServer : ISupportedDatabase
        {
            public TestcontainerDatabase TestcontainerDatabase { get; }

            public SqlServer()
            {
                TestcontainerDatabase = new TestcontainersBuilder<MsSqlTestcontainer>()
                    .WithDatabase(new MsSqlTestcontainerConfiguration
                    {
                        Password = "Qx#)T@pzKgAV^+tw"
                    })
                    .Build();
            }

            public void Configure(DbContextOptionsBuilder optionsBuilder, string databaseName)
            {
                optionsBuilder
                    .UseSqlServer(ReplaceDatabaseInConnectionString(
                        TestcontainerDatabase.ConnectionString,
                        databaseName));
            }

            private static string ReplaceDatabaseInConnectionString(
                string connectionString,
                string databaseName)
            {
                var databaseConnectionStringComponent = connectionString
                        .Split(new[] { ';' })
                        .SingleOrDefault(x => x.Contains("Database"));

                var replacementDatabaseConnectionStringComponent =
                    $"Database={databaseName}";

                return connectionString
                    .Replace(databaseConnectionStringComponent, replacementDatabaseConnectionStringComponent);
            }
        }

        public class PostgreSql: ISupportedDatabase
        {
            public TestcontainerDatabase TestcontainerDatabase { get; }

            public PostgreSql()
            {
                TestcontainerDatabase = new TestcontainersBuilder<PostgreSqlTestcontainer>()
                    .WithDatabase(new PostgreSqlTestcontainerConfiguration
                    {
                        Database = "db",
                        Username = "postgres",
                        Password = "postgres"
                    })
                    .Build();
            }

            public void Configure(
                DbContextOptionsBuilder optionsBuilder,
                string databaseName)
            {
                optionsBuilder
                    .UseNpgsql(ReplaceDatabaseInConnectionString(
                        TestcontainerDatabase.ConnectionString,
                        databaseName));
            }

            private static string ReplaceDatabaseInConnectionString(
                string connectionString,
                string databaseName)
            {
                var databaseConnectionStringComponent = connectionString
                        .Split(new[] { ';' })
                        .SingleOrDefault(x => x.Contains("Database"));

                var replacementDatabaseConnectionStringComponent =
                    $"Database={databaseName}";

                return connectionString
                    .Replace(databaseConnectionStringComponent, replacementDatabaseConnectionStringComponent);
            }
        }
    }
}
