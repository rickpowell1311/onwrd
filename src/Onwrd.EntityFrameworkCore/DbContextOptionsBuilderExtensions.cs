using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Onwrd.EntityFrameworkCore.Internal;
using Onwrd.EntityFrameworkCore.Internal.ContextConfiguration;

namespace Onwrd.EntityFrameworkCore
{
    public static class DbContextOptionsBuilderExtensions
    {
        public static void AddOutboxing(
            this DbContextOptionsBuilder dbContextOptionsBuilder,
            Action<OutboxingConfiguration> configuration)
        {
            var configured = new OutboxingConfiguration();
            configuration(configured);

            dbContextOptionsBuilder
                .AddInterceptors(
                    new SaveChangesInterceptor(configured.OnwardProcessorFactory));

            dbContextOptionsBuilder.ReplaceService<IModelCustomizer, OutboxingModelCustomizer>();
        }
    }
}
