using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public class EndToEndTests : IAsyncLifetime
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
        public async Task SaveChanges_ForSupportedDatabase_ProcessesEvent(ISupportedDatabase supportedDatabase)
        {
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig =>
                {
                    onwrdConfig.UseOnwardProcessor<TestOnwardProcessor>();
                    /* Because we have a unique database for each test, we can just let this context
                       control the creation of the schema */
                    onwrdConfig.RunMigrations = false;
                },
                ServiceLifetime.Transient);

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            await ExecuteEndToEndTest(serviceProvider);
        }

        private static async Task ExecuteEndToEndTest(IServiceProvider serviceProvider)
        {
            // Run migrations
            var context = serviceProvider.GetService<TestContext>();
            await context.Database.EnsureCreatedAsync();

            var testEntity = new TestEntity();
            testEntity.RaiseEvent();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            var processedEventAudit = serviceProvider.GetService<ProcessedEventAudit>();

            Assert.Single(processedEventAudit.ProcessedEvents);
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

                testEntityConfig.ToTable("TestEntity", "EndToEndTests");

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

            public Task Process<T>(T @event, EventMetadata eventMetadata)
            {
                this.processedEventAudit.Audit(@event);

                return Task.CompletedTask;
            }
        }

        internal class ProcessedEventAudit
        {
            private readonly List<object> _processedEvents;

            public IEnumerable<object> ProcessedEvents => _processedEvents;

            public ProcessedEventAudit()
            {
                _processedEvents = new List<object>();
            }

            public void Audit(object @event)
            {
                _processedEvents.Add(@event);
            }
        }
    }
}
