using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public class PublicTypesTests
    {
        [Fact]
        public void ImplementationNamespace_TypesInImplementationNamespace_AreInternal()
        {
            var internalTypesInPublicNamespace = typeof(EventRaiser).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace)
                    && x.Namespace.Contains("Onwrd.EntityFrameworkCore")
                    && !x.Namespace.Contains("Onwrd.EntityFrameworkCore.Internal")
                    && !x.IsNested
                    && !x.IsVisible);

            Assert.Empty(internalTypesInPublicNamespace);
        }
    }
}
