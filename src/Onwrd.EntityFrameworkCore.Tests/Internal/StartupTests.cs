using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class StartupTests : IAsyncLifetime
    {
        private TestcontainerDatabase database;

        public Task InitializeAsync()
        {
            // Initialization will be done in the tests
            return Task.CompletedTask;
        }

        public async Task DisposeAsync()
        {
            if (database != null)
            {
                await this.database.DisposeAsync();
            }
        }

        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task InitializeAsync_WhenUsingSupportedDatabaseConfiguration_DoesNotThrow(ISupportedDatabase supportedDatabase)
        {
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                outboxingConfig => { },
                ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();

            await startup.InitializeAsync();
        }

        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task Initialize_WhenUsingSupportedDatabaseConfiguration_DoesNotThrow(ISupportedDatabase supportedDatabase)
        {
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                outboxingConfig => { },
                ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            var startup = serviceProvider.GetService<Startup>();

            startup.Initialize();
        }

        internal class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }
        }
    }
}
