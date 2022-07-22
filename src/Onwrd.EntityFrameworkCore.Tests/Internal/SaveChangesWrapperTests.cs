using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class SaveChangesWrapperTests
    {
        private readonly TestOnwardProcessor onwardProcessor;
        private readonly TestContext testContext;

        public SaveChangesWrapperTests()
        {
            onwardProcessor = new TestOnwardProcessor();
            testContext = new TestContext();
        }

        [Fact]
        public async Task SaveChangesAsync_WhenMessageInOutboxedEntity_AddsMessageToOutbox()
        {
            var entity = new TestEntity();
            entity.AddMessageToOutbox();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            var outboxMessages = await this.testContext.Set<OutboxMessage>().ToListAsync();

            Assert.Single(outboxMessages);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenMessageInOutboxedEntityAndOnwardProcessorConfigured_ProcessesMessage()
        {
            var entity = new TestEntity();
            entity.AddMessageToOutbox();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            Assert.Single(this.onwardProcessor.Sent);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenMessageInOutboxedEntityAndOnwardProcessingSuccessful_MarksMessageAsDispatched()
        {
            var entity = new TestEntity();
            entity.AddMessageToOutbox();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            var outboxMessage = await this.testContext.Set<OutboxMessage>().SingleAsync();

            Assert.True(outboxMessage.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenMessageInOutboxedEntityAndOnwardProcessingFails_MessageIsNotDispatched()
        {
            var entity = new TestEntity();
            entity.AddMessageToOutbox();

            this.testContext.TestEntities.Add(entity);
            this.onwardProcessor.ShouldThrow = true;

            await SaveChangesWrapper().SaveChangesAsync();

            var outboxMessage = await this.testContext.Set<OutboxMessage>().SingleAsync();

            Assert.False(outboxMessage.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenTwoMessagesInOutboxedEntity_AddsMessagesToOutbox()
        {
            var entity = new TestEntity();
            entity.AddMessageToOutbox();
            entity.AddMessageToOutbox();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            Assert.Equal(2, this.onwardProcessor.Sent.Count());
        }

        private SaveChangesWrapper SaveChangesWrapper()
        {
            return new SaveChangesWrapper(
                this.testContext,
                () => this.testContext.SaveChangesAsync(),
                this.onwardProcessor);
        }

        internal class TestContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestContext() : base(Options)
            {
            }

            private static DbContextOptions<TestContext> Options
            {
                get
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
                    optionsBuilder.AddOutboxing();

                    return optionsBuilder.Options;
                }
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
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
            public bool ShouldThrow { get; set; }

            private readonly List<(object Message, MessageMetadata Metadata)> _sent;

            public IEnumerable<(object Message, MessageMetadata Metadata)> Sent => _sent;

            public TestOnwardProcessor()
            {
                _sent = new List<(object Message, MessageMetadata Metadata)>();
            }

            public Task Process<T>(T message, MessageMetadata messageMetadata)
            {
                if (ShouldThrow)
                {
                    throw new Exception("Couldn't process the message :(");
                }

                _sent.Add((message, messageMetadata));

                return Task.CompletedTask;
            }
        }
    }
}
