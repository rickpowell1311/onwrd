using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class ListExtensionsTests
    {
        [Fact]
        public void TryPop_WhenNoElementsExist_ReturnsFalseAndOutputsDefaultElementValue()
        {
            var list = new List<int>();

            var result = list.TryPop(out var element);

            Assert.False(result);
            Assert.Equal(default(int), element);
        }

        [Fact]
        public void TryPop_WhenOneElementExists_ReturnsTrueAndOutputsElement()
        {
            var list = new List<int>
            {
                1
            };

            var result = list.TryPop(out var element);

            Assert.True(result);
            Assert.Equal(1, element);
        }

        [Fact]
        public void TryPop_WhenTwoElementsExist_ReturnsTrueAndOutputsFirstElementAndLeavesOtherElementInList()
        {
            var list = new List<int>
            {
                1,
                2
            };

            var result = list.TryPop(out var element);

            Assert.True(result);
            Assert.Equal(1, element);
            Assert.Single(list);
            Assert.Equal(2, list.Single());
        }
    }
}
