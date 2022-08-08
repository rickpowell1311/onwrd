using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Onwrd.EntityFrameworkCore.Internal.ContextConfiguration;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal static class DbContextOptionsBuilderExtensions
    {
        /// <summary>
        /// Extends the client db context model with the Onwrd model configuration.
        /// </summary>
        /// <param name="dbContextOptionsBuilder"></param>
        internal static void AddOnwrdModel(this DbContextOptionsBuilder dbContextOptionsBuilder)
        {
            dbContextOptionsBuilder.ReplaceService<IModelCustomizer, OnwrdModelCustomizer>();
        }
    }
}
