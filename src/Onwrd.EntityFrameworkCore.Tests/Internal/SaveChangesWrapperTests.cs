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
        public async Task SaveChangesAsync_WhenEventRaised_AddsEventToEvents()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            var events = await this.testContext.Set<Event>().ToListAsync();

            Assert.Single(events);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEventRaisedAndOnwardProcessorConfigured_ProcessesEvent()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            Assert.Single(this.onwardProcessor.Processed);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenOnwardProcessingOfEventIsSuccessful_MarksEventAsDispatched()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            var @event = await this.testContext.Set<Event>().SingleAsync();

            Assert.True(@event.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenRaisedEventOnwardProcessingFails_EventIsNotDispatched()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            this.testContext.TestEntities.Add(entity);
            this.onwardProcessor.ShouldThrow = true;

            await SaveChangesWrapper().SaveChangesAsync();

            var @event = await this.testContext.Set<Event>().SingleAsync();

            Assert.False(@event.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenTwoEventsRaisedByEntity_ProcessesEvents()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();
            entity.RaiseEvent();

            this.testContext.TestEntities.Add(entity);

            await SaveChangesWrapper().SaveChangesAsync();

            Assert.Equal(2, this.onwardProcessor.Processed.Count());
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
                    optionsBuilder.AddOnwrdModel();

                    return optionsBuilder.Options;
                }
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
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
            public bool ShouldThrow { get; set; }

            private readonly List<(object Event, EventMetadata Metadata)> _processed;

            public IEnumerable<(object Event, EventMetadata Metadata)> Processed => _processed;

            public TestOnwardProcessor()
            {
                _processed = new List<(object Event, EventMetadata Metadata)>();
            }

            public Task Process<T>(T @event, EventMetadata eventMetadata)
            {
                if (ShouldThrow)
                {
                    throw new Exception("Couldn't process the event :(");
                }

                _processed.Add((@event, eventMetadata));

                return Task.CompletedTask;
            }
        }
    }
}
