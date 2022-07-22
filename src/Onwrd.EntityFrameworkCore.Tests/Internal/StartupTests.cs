using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class StartupTests : IDisposable
    {
        private IServiceProvider serviceProvider;

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Initialize_WhenUsingInMemoryDatabaseConfiguration_DoesNotThrow(bool isAsync)
        {
            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    builder.UseInMemoryDatabase(databaseUniqueId);
                },
                outboxingConfig => { },
                ServiceLifetime.Transient);

            this.serviceProvider = services.BuildServiceProvider();
            var startup = this.serviceProvider.GetService<Startup>();

            if (isAsync)
            {
                await startup.InitializeAsync();
            }
            else
            {
                startup.Initialize();
            }
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
                        .UseSqlServer(SqlServerConnectionString.ForDatabase(databaseUniqueId));
                },
                outboxingConfig => { },
                ServiceLifetime.Transient);

            this.serviceProvider = services.BuildServiceProvider();
            var startup = this.serviceProvider.GetService<Startup>();

            if (isAsync)
            {
                await startup.InitializeAsync();
            }
            else
            {
                startup.Initialize();
            }           
        }

        public void Dispose()
        {
            if (this.serviceProvider != null)
            {
                var context = this.serviceProvider.GetRequiredService<TestContext>();
                context.Database.EnsureDeleted();
            }
        }

        internal class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }
        }
    }
}
