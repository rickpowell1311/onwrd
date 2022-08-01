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
        public async Task SaveChanges_ForSupportedDatabase_DispatchesMessage(ISupportedDatabase supportedDatabase)
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
                outboxingConfig =>
                {
                    outboxingConfig.UseOnwardProcessor<TestOnwardProcessor>();
                    /* Because we have a unique database for each test, we can just let this context
                       control the creation of the schema */
                    outboxingConfig.RunMigrations = false;
                },
                ServiceLifetime.Transient);

            services.AddSingleton<SentMessageAudit>();

            var serviceProvider = services.BuildServiceProvider();

            await ExecuteEndToEndTest(serviceProvider);
        }

        private static async Task ExecuteEndToEndTest(IServiceProvider serviceProvider)
        {
            // Run migrations
            var context = serviceProvider.GetService<TestContext>();
            await context.Database.EnsureCreatedAsync();

            var testEntity = new TestEntity();
            testEntity.AddMessageToOutbox();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            var sentMessageAudit = serviceProvider.GetService<SentMessageAudit>();

            Assert.Single(sentMessageAudit.SentMessages);
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

        internal class TestEntity : Outboxed
        {
            public Guid Id { get; set; }

            public TestEntity()
            {
                Id = Guid.NewGuid();
            }

            public void AddMessageToOutbox()
            {
                AddToOutbox(new TestMessage("TestEntity"));
            }
        }

        internal class TestMessage
        {
            public string Greeting { get; set; }

            public TestMessage()
            {
            }

            public TestMessage(string sender)
            {
                Greeting = $"Hello from {sender}";
            }
        }

        internal class TestOnwardProcessor : IOnwardProcessor
        {
            private readonly SentMessageAudit sentMessageAudit;

            public TestOnwardProcessor(SentMessageAudit sentMessageAudit)
            {
                this.sentMessageAudit = sentMessageAudit;
            }

            public Task Process<T>(T message, MessageMetadata messageMetadata)
            {
                this.sentMessageAudit.Audit(message);

                return Task.CompletedTask;
            }
        }

        internal class SentMessageAudit
        {
            private readonly List<object> _sentMessages;

            public IEnumerable<object> SentMessages => _sentMessages;

            public SentMessageAudit()
            {
                _sentMessages = new List<object>();
            }

            public void Audit(object message)
            {
                _sentMessages.Add(message);
            }
        }
    }
}
