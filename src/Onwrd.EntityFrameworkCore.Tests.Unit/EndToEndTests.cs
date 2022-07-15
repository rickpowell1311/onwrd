using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Unit
{
    public class EndToEndTests
    {
        private readonly ServiceProvider serviceProvider;
        private bool messageSent = false;

        public EndToEndTests()
        {
            var onwardProcessor = new TestOnwardProcessor();
            onwardProcessor.MessageSent += ((object message, MessageMetadata metadata) input) => messageSent = true;

            var services = new ServiceCollection();
            services.AddDbContext<TestContext>(builder =>
            {
                builder.UseInMemoryDatabase($"TestContext-{Guid.NewGuid()}");
                builder.AddOutboxing(cfg =>
                {
                    cfg.UseOnwardProcessor(() => onwardProcessor);
                });
            }, ServiceLifetime.Transient);

            this.serviceProvider = services.BuildServiceProvider();
        }

        [Fact]
        public async Task SaveChanges_ForInMemoryConfiguration_DoesNotThrow()
        {
            var context = serviceProvider.GetService<TestContext>();

            var testEntity = new TestEntity();
            testEntity.AddMessageToOutbox();

            context.TestEntities.Add(testEntity);

            await context.SaveChangesAsync();

            Assert.True(messageSent);
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
            public event Action<(object Message, MessageMetadata Metadata)> MessageSent;

            public Task Process<T>(T message, MessageMetadata messageMetadata)
            {
                MessageSent((message, messageMetadata));

                return Task.CompletedTask;
            }
        }
    }
}
