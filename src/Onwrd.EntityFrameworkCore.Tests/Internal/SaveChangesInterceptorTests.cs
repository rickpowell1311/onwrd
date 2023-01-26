using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class SaveChangesInterceptorTests
    {
        private readonly string databaseName;
        private readonly TestOnwardProcessorOrchestrator onwardProcessor;
        private readonly IOnwardProcessingUnitOfWork<TestContext> unitOfWork;

        public SaveChangesInterceptorTests()
        {
            this.databaseName = $"OnwardTest-{Guid.NewGuid()}";
            this.onwardProcessor = new TestOnwardProcessorOrchestrator();

            var services = new ServiceCollection();
            services.AddTransient(sp => Context());

            this.unitOfWork = new OnwardProcessingUnitOfWork<TestContext>(
                services.BuildServiceProvider(),
                this.onwardProcessor);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEventRaised_AddsEventToEvents()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            var context = Context();

            context.TestEntities.Add(entity);
            await context.SaveChangesAsync();

            var events = await Context().Set<Event>().ToListAsync();

            Assert.Single(events);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenEventRaisedAndOnwardProcessorConfigured_ProcessesEvent()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            var context = Context();

            context.TestEntities.Add(entity);
            await context.SaveChangesAsync();

            Assert.Single(this.onwardProcessor.Processed);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenOnwardProcessingOfEventIsSuccessful_MarksEventAsDispatched()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            var context = Context();

            context.TestEntities.Add(entity);
            await context.SaveChangesAsync();

            var @event = await Context().Set<Event>().SingleAsync();

            Assert.True(@event.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenRaisedEventOnwardProcessingFails_EventIsNotDispatched()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();

            var context = Context();
            context.TestEntities.Add(entity);

            this.onwardProcessor.ShouldThrow = true;

            await context.SaveChangesAsync();

            var @event = await Context().Set<Event>().SingleAsync();

            Assert.False(@event.DispatchedOn.HasValue);
        }

        [Fact]
        public async Task SaveChangesAsync_WhenTwoEventsRaisedByEntity_ProcessesEvents()
        {
            var entity = new TestEntity();
            entity.RaiseEvent();
            entity.RaiseEvent();

            var context = Context();

            context.TestEntities.Add(entity);
            await context.SaveChangesAsync();

            Assert.Equal(2, this.onwardProcessor.Processed.Count());
        }

        private TestContext Context()
        {
            return new TestContext(
                this.databaseName,
                this.unitOfWork);
        }

        internal class TestContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestContext(
                string databaseName,
                IOnwardProcessingUnitOfWork<TestContext> unitOfWork) : base(GetOptions(
                    databaseName,
                    unitOfWork))
            {
            }

            private static DbContextOptions<TestContext> GetOptions(
                string databaseName,
                IOnwardProcessingUnitOfWork<TestContext> unitOfWork)
            {
                var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
                optionsBuilder.UseInMemoryDatabase(databaseName);
                optionsBuilder.AddOnwrdModel();
                optionsBuilder.AddInterceptors(
                    new SaveChangesInterceptor<TestContext>(
                        unitOfWork));

                return optionsBuilder.Options;
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

        internal class TestOnwardProcessorOrchestrator : IOnwardProcessorOrchestrator
        {
            public bool ShouldThrow { get; set; }

            private readonly List<object> _processed;

            public IEnumerable<object> Processed => _processed;

            public TestOnwardProcessorOrchestrator()
            {
                _processed = new List<object>();
            }

            public Task Process((Event Event, object Contents) eventPair, IServiceScope _, CancellationToken cancellationToken = default)
            {
                if (ShouldThrow)
                {
                    throw new Exception("Couldn't process the event :(");
                }

                _processed.Add(eventPair.Contents);

                return Task.CompletedTask;
            }
        }
    }
}
