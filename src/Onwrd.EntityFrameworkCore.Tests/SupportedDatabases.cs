using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;
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

            public void Configure(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseSqlServer(TestcontainerDatabase.ConnectionString);
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
                DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder
                    .UseNpgsql(TestcontainerDatabase.ConnectionString);
            }
        }
    }
}
