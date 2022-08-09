using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
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
            var databaseName = $"OnwrdTest-720D6C5E-9F1A-43DB-90F6-685562D087CC";

            // Start the container 
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            /* Create a version of the database without any Onwrd schema by configuring a context
             * with has no onwrd models added */
            var contextBuilder = new DbContextOptionsBuilder<TestContext>();
            supportedDatabase.Configure(contextBuilder, databaseName);

            var databaseCreationContext = new TestContext(contextBuilder.Options);
            await databaseCreationContext.Database.EnsureCreatedAsync();

            // Configure the context with Onwrd in a service provider
            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder, databaseName);
                },
                onwrdConfig =>
                {
                    onwrdConfig.UseOnwardProcessor<TestOnwardProcessor>();
                },
                ServiceLifetime.Transient);

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            // Run the test
            await ExecuteEndToEndTest(serviceProvider);
        }

        private static async Task ExecuteEndToEndTest(IServiceProvider serviceProvider)
        {
            var context = serviceProvider.GetService<TestContext>();

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
