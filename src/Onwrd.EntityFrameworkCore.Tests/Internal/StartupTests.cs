using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class SqlServerStartupTests : StartupTests
    {
        public SqlServerStartupTests() : base(Database)
        {
        }

        private static TestcontainerDatabase Database => new TestcontainersBuilder<MsSqlTestcontainer>()
            .WithDatabase(new MsSqlTestcontainerConfiguration
            {
                Password = "Qx#)T@pzKgAV^+tw",
            })
            .Build();

        protected override string ReplaceDatabaseInConnectionString(
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

    public abstract class StartupTests : IAsyncLifetime
    {
        private readonly TestcontainerDatabase testcontainerDatabase;

        public StartupTests(TestcontainerDatabase database)
        {
            this.testcontainerDatabase = database;
        }

        public async Task InitializeAsync()
        {
            await this.testcontainerDatabase.StartAsync();
        }

        public async Task DisposeAsync()
        {
            await this.testcontainerDatabase.DisposeAsync();
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Initialize_WhenUsingSqlServerDatabaseConfiguration_DoesNotThrow(bool isAsync)
        {
            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    builder
                        .UseSqlServer(ReplaceDatabaseInConnectionString(
                            this.testcontainerDatabase.ConnectionString,
                            databaseUniqueId));
                },
                outboxingConfig => { },
                ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();

            if (isAsync)
            {
                await startup.InitializeAsync();
            }
            else
            {
                startup.Initialize();
            }
        }

        protected abstract string ReplaceDatabaseInConnectionString(
            string connectionString,
            string databaseName);

        internal class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }
        }
    }
}
