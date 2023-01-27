using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
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
                await database.DisposeAsync();
            }
        }

        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task InitializeAsync_WhenUsingSupportedDatabaseConfiguration_DoesNotThrow(ISupportedDatabase supportedDatabase)
        {
            database = supportedDatabase.TestcontainerDatabase;
            await database.StartAsync();

            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig => { },
                ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();

            // Verify mapping to Event DbSet is ok
            using var context = serviceProvider.GetService<TestContext>();
            _ = await context.Set<Event>().ToListAsync();
        }

        internal class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.AddOnwrdModel();
                base.OnModelCreating(modelBuilder);
            }
        }
    }
}
