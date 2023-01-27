using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal;
using Onwrd.EntityFrameworkCore.Internal.Migrations;

namespace Onwrd.EntityFrameworkCore
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDbContext<TContext>(
            this IServiceCollection serviceCollection,
            Action<IServiceProvider, DbContextOptionsBuilder> optionsAction,
            Action<OnwrdConfiguration> onwrdConfiguration,
            ServiceLifetime contextLifetime = ServiceLifetime.Scoped,
            ServiceLifetime optionsLifetime = ServiceLifetime.Scoped)
            where TContext : DbContext
        {
            // Core onwrd services
            var config = new OnwrdConfiguration();
            onwrdConfiguration(config);

            serviceCollection.AddSingleton(config);

            serviceCollection.AddScoped<SaveChangesInterceptor<TContext>>();
            serviceCollection.AddTransient<IOnwardProcessingUnitOfWork<TContext>, OnwardProcessingUnitOfWork<TContext>>();
            serviceCollection.AddTransient<IOnwardProcessorOrchestrator, OnwardProcessorOrchestrator>();
            serviceCollection.AddTransient<OnConnectingInterceptor>();
            serviceCollection.AddSingleton<RunOnce>();
            serviceCollection.AddTransient<IWait, Wait>();

            // Onward processors
            if (config.UseOnwrdProcessor && !serviceCollection.Any(x => x.ServiceType == config.OnwardProcessorType))
            {
                serviceCollection.AddTransient(typeof(IOnwardProcessor), config.OnwardProcessorType);
            }

            if (config.UseOnwrdProcessors)
            {
                foreach (var (type, processorTypes) in config.OnwrdProcessorsConfiguration.Library.Entries)
                {
                    var serviceType = typeof(IOnwardProcessor<>)
                        .MakeGenericType(type);

                    foreach (var processorType in processorTypes)
                    {
                        serviceCollection.AddTransient(serviceType, processorType);
                    }
                }
            }

            // Retries
            serviceCollection.AddTransient<IOnwardRetryManager<TContext>, OnwardRetryManager<TContext>>();
            serviceCollection.AddSingleton(config.RetryConfiguration);

            void optionsActionOverride(IServiceProvider serviceProvider, DbContextOptionsBuilder builder)
            {
                optionsAction(serviceProvider, builder);
                builder.AddInterceptors(
                    serviceProvider.GetRequiredService<SaveChangesInterceptor<TContext>>(),
                    serviceProvider.GetRequiredService<OnConnectingInterceptor>());
            }

            serviceCollection.AddDbContext<TContext>(
                optionsActionOverride,
                contextLifetime,
                optionsLifetime);

            // Migrations
            var migrationsServiceCollection = new ServiceCollection();
            migrationsServiceCollection.AddDbContext<TContext>(optionsAction);
            using var migrationsServiceProvider = migrationsServiceCollection.BuildServiceProvider();
            using var migrationsScope = migrationsServiceProvider.CreateScope();
            using var migrationsContext = migrationsScope.ServiceProvider.GetService<TContext>();
            var providerName = migrationsContext.Database.ProviderName;

            serviceCollection.AddTransient<IOnwrdMigrator>(sp => new OnwrdMigrator(providerName));

            return serviceCollection;
        }
    }
}
