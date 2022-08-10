using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class InternalTypesTests
    {
        [Fact]
        public void ImplementationNamespace_TypesInImplementationNamespace_AreInternal()
        {
            var publicTypesInInternalNamespace = typeof(Event).Assembly.GetTypes()
                .Where(x => !string.IsNullOrWhiteSpace(x.Namespace) 
                    && x.Namespace.Contains("Onwrd.EntityFrameworkCore.Internal")
                    && x.IsVisible);

            Assert.Empty(publicTypesInInternalNamespace);
        }
    }
}