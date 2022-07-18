using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    internal static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddOutboxedDbContext<TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            Action<OutboxingConfiguration> outboxingConfiguration,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            var config = new OutboxingConfiguration(serviceCollection);
            outboxingConfiguration(config);

            serviceCollection.AddTransient<SaveChangesInterceptor>();

            Action<IServiceProvider, DbContextOptionsBuilder> optionsActionOverride = (IServiceProvider serviceProvider, DbContextOptionsBuilder builder) =>
            {
                optionsAction(serviceProvider, builder);
                builder.AddOutboxing();
                builder.AddInterceptors(serviceProvider.GetRequiredService<SaveChangesInterceptor>());
            };

            serviceCollection.AddDbContext<TContext>(
                optionsActionOverride,
                contextLifetime,
                optionsLifetime);

            return serviceCollection;
        }
    }
}
