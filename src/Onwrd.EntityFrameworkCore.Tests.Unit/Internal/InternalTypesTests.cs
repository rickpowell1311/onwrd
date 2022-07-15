using Xunit;

namespace Onwrd.EntityFrameworkCore.Internal
{
    public class InternalTypesTests
    {
        [Fact]
        public void ImplementationNamespace_TypesInImplementationNamespace_AreInternal()
        {
            var publicTypesInImplementationNamespace = typeof(OutboxMessage).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace) 
                    && x.Namespace.Contains("Onwrd.EntityFrameworkCore.Internal")
                    && x.IsVisible);

            Assert.Empty(publicTypesInImplementationNamespace);
        }
    }
}