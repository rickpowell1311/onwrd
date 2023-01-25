using System.Runtime.CompilerServices;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public class OnwrdProcessorsConfigurationTests
    {
        [Fact]
        public void Register_WhenAlreadyRegistered_OnlyHasOneRegistrationInLibrary()
        {
            var onwrdProcessorsConfiguration = new OnwrdProcessorsConfiguration();
            onwrdProcessorsConfiguration.Register<TestEvent, TestProcessor>();
            onwrdProcessorsConfiguration.Register<TestEvent, TestProcessor>();

            Assert.Single(onwrdProcessorsConfiguration
                .Library[typeof(TestEvent)]);
        }

        [Fact]
        public void Register_WhenScanningAssembly_IncludesTestEventAndTestProcessorsInLibrary()
        {
            var onwrdProcessorsConfiguration = new OnwrdProcessorsConfiguration();
            onwrdProcessorsConfiguration.ScanAssemblies(typeof(TestProcessor).Assembly);

            var result = onwrdProcessorsConfiguration.Library[typeof(TestEvent)];

            Assert.Single(result);
        }

        public class TestEvent { }

        public class TestProcessor : IOnwardProcessor<TestEvent>
        {
            public bool HasProcessed { get; private set; }

            public Task Process(TestEvent @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default)
            {
                HasProcessed = true;

                return Task.CompletedTask;
            }
        }
    }
}
