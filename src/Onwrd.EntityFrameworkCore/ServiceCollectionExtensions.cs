using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;
using Onwrd.EntityFrameworkCore.Internal.Migrations;
using Onwrd.EntityFrameworkCore.Internal.Migrations.Updates;

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
            serviceCollection.AddTransient<Startup>();
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

            // Migration services
            var migrationServiceCollection = new ServiceCollection();

            void migrationOptionsActionOverride(IServiceProvider serviceProvider, DbContextOptionsBuilder builder)
            {
                optionsAction(serviceProvider, builder);
                builder.ReplaceService<IMigrator, OnwrdMigrator>();
            }

            migrationServiceCollection.AddDbContext<MigrationContext>(
                migrationOptionsActionOverride,
                ServiceLifetime.Transient,
                ServiceLifetime.Transient);

            var migrationServices = new MigrationServices(migrationServiceCollection.BuildServiceProvider());
            serviceCollection.AddSingleton(migrationServices);

            return serviceCollection;
        }
    }
}
