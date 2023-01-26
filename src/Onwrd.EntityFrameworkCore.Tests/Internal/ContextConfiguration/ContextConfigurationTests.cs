using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Xunit;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore.Tests.Internal.ContextConfiguration
{
    public class ContextConfigurationTests
    {
        [Fact]
        public async Task Customize_WhenUsingTestModelCustomizer_ConfiguresCustomAndOnwrdDbSets()
        {
            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    builder.UseInMemoryDatabase(databaseUniqueId);
                    builder.ReplaceService<IModelCustomizer, TestModelCustomizer>();
                },
                onwrdConfig => { },
                ServiceLifetime.Transient);

            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            using var context = scope.ServiceProvider.GetService<TestContext>();

            // Neither query should throw if the model is configured correctly
            await context.Set<Event>().AnyAsync();
            await context.Set<CustomizedEntity>().AnyAsync();
        }
        
        internal class TestContext : DbContext 
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options) { }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.AddOnwrdModel();
                base.OnModelCreating(modelBuilder);
            }
        }

        internal class TestModelCustomizer : ModelCustomizer
        {
            public TestModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
            {

            }

            public override void Customize(ModelBuilder modelBuilder, DbContext context)
            {
                modelBuilder.Entity<CustomizedEntity>(cfg =>
                {
                    cfg.HasKey(x => x.Id);
                });

                base.Customize(modelBuilder, context);
            }
        }

        public class CustomizedEntity
        {
            public Guid Id { get; set; }
        }
    }
}
