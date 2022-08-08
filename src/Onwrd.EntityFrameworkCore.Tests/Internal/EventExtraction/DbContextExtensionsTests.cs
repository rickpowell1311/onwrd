using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.EventExtraction;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal.EventExtraction
{
    public class DbContextExtensionsTests
    {
        [Fact]

        public void ExtractEvents_FromAddedEntityWithRaisedEvents_ReturnsEvents()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.RaiseEvent();

            context.TestEntities.Add(entity);

            var result = context.ExtractEvents()
                .SingleOrDefault(x => x is RaisedEvent m && m.Greeting == "Hello from TestEntity");

            Assert.NotNull(result);
        }

        [Fact]
        public async void ExtractEvents_FromUpdatedEntityWithRaisedEvent_ReturnsEvent()
        {
            var context = new TestContext();
            var entity = new TestEntity();

            context.TestEntities.Add(entity);

            await context.SaveChangesAsync();

            entity = await context.TestEntities.FindAsync(entity.Id);
            entity.RaiseEvent();

            var result = context.ExtractEvents()
                .SingleOrDefault(x => x is RaisedEvent m && m.Greeting == "Hello from TestEntity");

            Assert.NotNull(result);
        }

        [Fact]

        public void ExtractEvents_FromAddedEntityWithAddedNavigationEntityWithRaisedEvent_ReturnsEvent()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.NavigationEntity = new TestNavigationEntity();
            entity.NavigationEntity.RaiseEvent();

            context.TestEntities.Add(entity);

            var result = context.ExtractEvents()
                .SingleOrDefault(x => x is RaisedEvent e && e.Greeting == "Hello from TestNavigationEntity");

            Assert.NotNull(result);
        }

        [Fact]
        public async void ExtractEvents_FromUpdatedEntityWithAddedNavigationEntityWithRaisedEvent_ReturnsEvent()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.NavigationEntity = new TestNavigationEntity();

            context.TestEntities.Add(entity);

            await context.SaveChangesAsync();

            entity = context.TestEntities.Find(entity.Id);
            entity.NavigationEntity.RaiseEvent();

            var result = context.ExtractEvents()
                .SingleOrDefault(x => x is RaisedEvent m && m.Greeting == "Hello from TestNavigationEntity");

            Assert.NotNull(result);
        }

        internal class TestContext : DbContext
        {
            public DbSet<TestEntity> TestEntities { get; set; }

            public TestContext()
                : base(Options)
            {
            }

            private static DbContextOptions<TestContext> Options
            {
                get
                {
                    var optionsBuilder = new DbContextOptionsBuilder<TestContext>();
                    optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());

                    return optionsBuilder.Options;
                }
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);

                modelBuilder.Entity<TestEntity>()
                    .OwnsOne(x => x.NavigationEntity);
            }
        }

        internal class TestEntity : EventRaiser
        {
            public Guid Id { get; set; }

            internal TestNavigationEntity NavigationEntity { get; set; }

            public TestEntity()
            {
                Id = Guid.NewGuid();
            }

            public void RaiseEvent()
            {
                RaiseEvent(new RaisedEvent("TestEntity"));
            }
        }

        internal class TestNavigationEntity : EventRaiser
        {
            public Guid Id { get; set; }

            public TestNavigationEntity()
            {
                Id = Guid.NewGuid();

                RaiseEvent(new RaisedEvent("TestNavigationEntity"));
            }

            public void RaiseEvent()
            {
                RaiseEvent(new RaisedEvent("TestEntity"));
            }
        }

        internal class RaisedEvent
        {
            public string Greeting { get; set; }

            public RaisedEvent()
            {
            }

            public RaisedEvent(string sender)
            {
                Greeting = $"Hello from {sender}";
            }
        }
    }
}
