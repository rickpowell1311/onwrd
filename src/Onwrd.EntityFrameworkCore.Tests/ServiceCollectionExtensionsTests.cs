using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static Onwrd.EntityFrameworkCore.Tests.EndToEndTests;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public class ServiceCollectionExtensionsTests
    {
        [Fact]
        public void AddDbContext_WhenScanningForOnwardProcessors_DoesNotThrow()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    builder.UseInMemoryDatabase("ServiceCollectionExtensionsTests");
                }, onwrd =>
                {
                    onwrd.UseOnwardProcessors(cfg =>
                    {
                        cfg.ScanAssemblies(typeof(ServiceCollectionExtensionsTests).Assembly);
                    });
                });
        }

        internal class TestContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var testEntityConfig = modelBuilder.Entity<TestEntity>();

                testEntityConfig.ToTable("TestEntity", "ServiceCollectionExtensionsTests");

                testEntityConfig.HasKey(x => x.Id);

                testEntityConfig
                    .Property(x => x.Id)
                    .ValueGeneratedNever();

                base.OnModelCreating(modelBuilder);
            }
        }

        internal class TestEntity : EventRaiser
        {
            public Guid Id { get; set; }

            public TestEntity()
            {
                Id = Guid.NewGuid();
            }

            public void RaiseEvent()
            {
                RaiseEvent(new TestEvent("TestEntity"));
            }
        }

        internal class TestEvent
        {
            public string Greeting { get; set; }

            public TestEvent()
            {
            }

            public TestEvent(string sender)
            {
                Greeting = $"Hello from {sender}";
            }
        }

        internal class TestOnwardProcessor : IOnwardProcessor
        {
            private readonly ProcessedEventAudit processedEventAudit;

            public TestOnwardProcessor(ProcessedEventAudit processedEventAudit)
            {
                this.processedEventAudit = processedEventAudit;
            }

            public Task Process<T>(T @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default)
            {
                this.processedEventAudit.Audit(@event);

                return Task.CompletedTask;
            }
        }
    }
}
