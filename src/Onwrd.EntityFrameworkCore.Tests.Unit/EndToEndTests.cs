using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Unit
{
    public class EndToEndTests
    {
        private readonly ServiceProvider serviceProvider;

        public EndToEndTests()
        {
            var services = new ServiceCollection();
            services.AddOutboxedDbContext<TestContext>(
                (_, builder) =>
                {
                    builder.UseInMemoryDatabase($"TestContext-{Guid.NewGuid()}");
                },
                outboxingConfig =>
                {
                    outboxingConfig.UseOnwardProcessor<TestOnwardProcessor>();
                },
                ServiceLifetime.Transient);

            services.AddSingleton<SentMessageAudit>();

            this.serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task SaveChanges_ForInMemoryConfiguration_DispatchesMessage()
        {
            var context = serviceProvider.GetService<TestContext>();

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
