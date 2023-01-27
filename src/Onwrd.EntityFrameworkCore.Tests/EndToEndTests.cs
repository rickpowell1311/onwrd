using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using System.Threading.Tasks;
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
            // Start the container 
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            /* Create the database by configuring the context */
            var contextBuilder = new DbContextOptionsBuilder<TestContext>();
            supportedDatabase.Configure(contextBuilder);

            var databaseCreationContext = new TestContext(contextBuilder.Options);
            await databaseCreationContext.Database.EnsureCreatedAsync();

            // Configure the context with Onwrd in a service provider
            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig =>
                {
                    onwrdConfig.RunMigrations = false;
                    onwrdConfig.UseOnwardProcessor<TestOnwardProcessor>();
                });

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetService<TestContext>();

            var testEntity = new TestEntity();
            testEntity.RaiseEvent();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            var processedEventAudit = serviceProvider.GetService<ProcessedEventAudit>();

            Assert.Single(processedEventAudit.ProcessedEvents);
        }

        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task SaveChanges_ForSupportedDatabase_ProcessesEventWithIndividualOnwardProcessor(ISupportedDatabase supportedDatabase)
        {
            // Start the container 
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            /* Create the database by configuring the context */
            var contextBuilder = new DbContextOptionsBuilder<TestContext>();
            supportedDatabase.Configure(contextBuilder);

            var databaseCreationContext = new TestContext(contextBuilder.Options);
            await databaseCreationContext.Database.EnsureCreatedAsync();

            // Configure the context with Onwrd in a service provider
            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig =>
                {
                    onwrdConfig.RunMigrations = false;
                    onwrdConfig.UseOnwardProcessors(pcsConfig =>
                    {
                        pcsConfig.Register<TestEvent, TestEventOnwardProcessor>();
                    });
                });

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetService<TestContext>();

            var testEntity = new TestEntity();
            testEntity.RaiseEvent();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            var processedEventAudit = serviceProvider.GetService<ProcessedEventAudit>();

            Assert.Single(processedEventAudit.ProcessedEvents);
        }

        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task RetryOnwardProcessing_ForSupportedDatabaseWithUnprocessedEvent_ProcessesEvent(
            ISupportedDatabase supportedDatabase)
        {
            // Start the container 
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            /* Create the database by configuring the context */
            var contextBuilder = new DbContextOptionsBuilder<TestContext>();
            supportedDatabase.Configure(contextBuilder);

            var databaseCreationContext = new TestContext(contextBuilder.Options);
            await databaseCreationContext.Database.EnsureCreatedAsync();

            // Configure the context with Onwrd in a service provider
            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig =>
                {
                    onwrdConfig.RunMigrations = false;
                    onwrdConfig.UseOnwardProcessor<TestOnwardProcessor>();
                    onwrdConfig.ConfigureRetryOptions(retryConfig =>
                    {
                        retryConfig.MaximumRetryAttempts = 3;
                        retryConfig.RetryAfter = TimeSpan.FromSeconds(0);
                    });
                });

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetRequiredService<TestContext>();
            context.RaiseEvent(new TestEvent("Test"));

            await context.SaveChangesAsync();

            /* Mark the event as unprocessed, and clear the audit so that the retry can be tested for an existing event */
            serviceProvider.GetService<ProcessedEventAudit>().Clear();
            var @event = await context.Set<Event>().SingleAsync();
            await context.Entry(@event).ReloadAsync();
            @event.DispatchedOn = null;

            await context.SaveChangesAsync();

            var existingEvents = await context.Set<Event>().ToListAsync();
            Assert.Single(existingEvents);

            /* Ensure the retry manager processes the message */
            var retryManager = serviceProvider.GetService<IOnwardRetryManager<TestContext>>();

            await retryManager.RetryOnwardProcessing(CancellationToken.None);

            Assert.Single(serviceProvider.GetService<ProcessedEventAudit>().ProcessedEvents);
        }


        [Theory]
        [MemberData(nameof(SupportedDatabases.All), MemberType = typeof(SupportedDatabases))]
        public async Task SaveChanges_WhenEventAddedDirectlyToContextForSupportedDatabase_ProcessesEvent(ISupportedDatabase supportedDatabase)
        {
            // Start the container 
            this.database = supportedDatabase.TestcontainerDatabase;
            await this.database.StartAsync();

            /* Create the database by configuring the context */
            var contextBuilder = new DbContextOptionsBuilder<TestContext>();
            supportedDatabase.Configure(contextBuilder);

            var databaseCreationContext = new TestContext(contextBuilder.Options);
            await databaseCreationContext.Database.EnsureCreatedAsync();

            // Configure the context with Onwrd in a service provider
            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(
                (_, builder) =>
                {
                    supportedDatabase.Configure(builder);
                },
                onwrdConfig =>
                {
                    onwrdConfig.RunMigrations = false;
                    onwrdConfig.UseOnwardProcessor<TestOnwardProcessor>();
                });

            services.AddSingleton<ProcessedEventAudit>();

            var serviceProvider = services.BuildServiceProvider();

            var context = serviceProvider.GetService<TestContext>();
            context.RaiseEvent(new TestEvent { Greeting = "Hello!" });

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

                modelBuilder.AddOnwrdModel();
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

        internal class TestEventOnwardProcessor : IOnwardProcessor<TestEvent>
        {
            private readonly ProcessedEventAudit processedEventAudit;

            public TestEventOnwardProcessor(ProcessedEventAudit processedEventAudit)
            {
                this.processedEventAudit = processedEventAudit;
            }

            public Task Process(TestEvent @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default)
            {
                this.processedEventAudit.Audit(@event);

                return Task.CompletedTask;
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

            public void Clear()
            {
                _processedEvents.Clear();
            }
        }
    }
}
