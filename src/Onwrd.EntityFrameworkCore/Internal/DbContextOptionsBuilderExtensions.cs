using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Onwrd.EntityFrameworkCore.Internal.ContextConfiguration;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal static class DbContextOptionsBuilderExtensions
    {
        internal static void AddOutboxing(this DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.ReplaceService<IModelCustomizer, OutboxingModelCustomizer>();
        }
    }
}
