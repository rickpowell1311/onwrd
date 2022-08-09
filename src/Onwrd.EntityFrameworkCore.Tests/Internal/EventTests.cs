using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class EventTests
    {
        [Fact]
        public void DeserializeContents_WhenTypeIdCannotBeMatchedToAType_ThrowsTypeLoadException()
        {
            var @event = new TestEvent();

            var storedEvent = Event.FromContents(@event);
            storedEvent.TypeId = $"NonsenseABC123";

            Assert.Throws<TypeLoadException>(() => storedEvent.DeserializeContents());
        }

        [Fact]
        public void DeserializeContents_WhenAssemblyNameCannotBeMatchedToAnAssembly_ThrowsTypeLoadException()
        {
            var @event = new TestEvent();

            var storedEvent = Event.FromContents(@event);
            storedEvent.AssemblyName = $"NonsenseXYZ987";

            Assert.Throws<TypeLoadException>(() => storedEvent.DeserializeContents());
        }

        [Fact]
        public void DeserializeContents_AfterSerializing_ReturnsContents()
        {
            var @event = new TestEvent();

            var storedEvent = Event.FromContents(@event);

            var deserialized = storedEvent.DeserializeContents();
            Assert.IsType<TestEvent>(deserialized);

            Assert.Equal(@event.Id, ((TestEvent)deserialized).Id);
        }

        private class TestEvent
        {
            public Guid Id { get; set; }

            public TestEvent()
            {
                Id = Guid.NewGuid();
            }
        }
    }
}
