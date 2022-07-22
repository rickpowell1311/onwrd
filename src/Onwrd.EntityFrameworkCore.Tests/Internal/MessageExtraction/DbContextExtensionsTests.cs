using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.MessageExtraction;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal.MessageExtraction
{
    public class DbContextExtensionsTests
    {
        [Fact]

        public void ExtractMessages_FromAddedEntityWithMessageInOutbox_ReturnsMessage()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.AddMessageToOutbox();

            context.TestEntities.Add(entity);

            var result = context.ExtractMessages()
                .SingleOrDefault(x => x is TestMessage m && m.Greeting == "Hello from TestEntity");

            Assert.NotNull(result);
        }

        [Fact]
        public async void ExtractMessages_FromUpdatedEntityWithMessageInOutbox_ReturnsMessage()
        {
            var context = new TestContext();
            var entity = new TestEntity();

            context.TestEntities.Add(entity);

            await context.SaveChangesAsync();

            entity = await context.TestEntities.FindAsync(entity.Id);
            entity.AddMessageToOutbox();

            var result = context.ExtractMessages()
                .SingleOrDefault(x => x is TestMessage m && m.Greeting == "Hello from TestEntity");

            Assert.NotNull(result);
        }

        [Fact]

        public void ExtractMessages_FromAddedEntityWithAddedNavigationEntityWithMessageInOutbox_ReturnsMessage()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.NavigationEntity = new TestNavigationEntity();
            entity.NavigationEntity.AddMessageToOutbox();

            context.TestEntities.Add(entity);

            var result = context.ExtractMessages()
                .SingleOrDefault(x => x is TestMessage m && m.Greeting == "Hello from TestNavigationEntity");

            Assert.NotNull(result);
        }

        [Fact]
        public async void ExtractMessages_FromUpdatedEntityWithAddedNavigationEntityWithMessageInOutbox_ReturnsMessage()
        {
            var context = new TestContext();
            var entity = new TestEntity();
            entity.NavigationEntity = new TestNavigationEntity();

            context.TestEntities.Add(entity);

            await context.SaveChangesAsync();

            entity = context.TestEntities.Find(entity.Id);
            entity.NavigationEntity.AddMessageToOutbox();

            var result = context.ExtractMessages()
                .SingleOrDefault(x => x is TestMessage m && m.Greeting == "Hello from TestNavigationEntity");

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

        internal class TestEntity : Outboxed
        {
            public Guid Id { get; set; }

            internal TestNavigationEntity NavigationEntity { get; set; }

            public TestEntity()
            {
                Id = Guid.NewGuid();
            }

            public void AddMessageToOutbox()
            {
                AddToOutbox(new TestMessage("TestEntity"));
            }
        }

        internal class TestNavigationEntity : Outboxed
        {
            public Guid Id { get; set; }

            public TestNavigationEntity()
            {
                Id = Guid.NewGuid();

                AddToOutbox(new TestMessage("TestNavigationEntity"));
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
    }
}
