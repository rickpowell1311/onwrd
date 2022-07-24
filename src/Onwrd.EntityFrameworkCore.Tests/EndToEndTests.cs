using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public class EndToEndTests : IDisposable
    {
        private IServiceProvider serviceProvider;

        public void Dispose()
        {
            if (this.serviceProvider != null)
            {
                var context = this.serviceProvider.GetRequiredService<TestContext>();
                context.Database.EnsureDeleted();
            }
        }

        [Fact]
        public async Task SaveChanges_ForInMemoryConfiguration_DispatchesMessage()
        {
            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    builder.UseInMemoryDatabase(databaseUniqueId);
                },
                outboxingConfig =>
                {
                    outboxingConfig.UseOnwardProcessor<TestOnwardProcessor>();
                },
                ServiceLifetime.Transient);

            services.AddSingleton<SentMessageAudit>();

            this.serviceProvider = services.BuildServiceProvider();

            await ExecuteEndToEndTest();
        }

        [Fact]
        public async Task SaveChanges_ForSqlServerConfiguration_DispatchesMessage()
        {
            var services = new ServiceCollection();
            var databaseUniqueId = $"onward-{Guid.NewGuid()}";
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    builder
                        .UseSqlServer(SqlServerConnectionString.ForDatabase(databaseUniqueId));
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

            this.serviceProvider = services.BuildServiceProvider();

            await ExecuteEndToEndTest();
        }

        [Fact]
        public void Temp()
        {
            throw new Exception("Oops");
        }

        private async Task ExecuteEndToEndTest()
        {
            // Run migrations
            var context = this.serviceProvider.GetService<TestContext>();
            await context.Database.EnsureCreatedAsync();

            var testEntity = new TestEntity();
            testEntity.AddMessageToOutbox();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            var sentMessageAudit = this.serviceProvider.GetService<SentMessageAudit>();

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
