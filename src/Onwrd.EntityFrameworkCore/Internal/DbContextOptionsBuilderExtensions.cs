using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations.Internal;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal.ContextConfiguration;
using Onwrd.EntityFrameworkCore.Internal.Migrations;

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
            ((IDbContextOptionsBuilderInfrastructure)dbContextOptionsBuilder)
                .AddOrUpdateExtension(new OnwrdDbContextOptionsExtension());
        }

        internal class OnwrdDbContextOptionsExtension :  IDbContextOptionsExtension
        {
            public DbContextOptionsExtensionInfo Info => new ExtensionInfo(this);

            public void ApplyServices(IServiceCollection services)
            {
                if (services is null)
                {
                    throw new ArgumentNullException(nameof(services));
                }

                for (var index = services.Count - 1; index >= 0; index--)
                {
                    var descriptor = services[index];
                    if (descriptor.ServiceType != typeof(IModelCustomizer))
                    {
                        continue;
                    }

                    if (descriptor.ImplementationType is null)
                    {
                        continue;
                    }

                    // Register as implementation type rather than as service type
                    services[index] = new ServiceDescriptor(
                        descriptor.ImplementationType,
                        descriptor.ImplementationType,
                        descriptor.Lifetime
                    );

                    object GetOnwrdModelCustomizer(IServiceProvider sp)
                    {
                        var inner = sp.GetService(descriptor.ImplementationType) as ModelCustomizer;

                        var modelCustomizerDependenciesProperty = typeof(ModelCustomizer)
                            .GetProperty(
                                "Dependencies",
                                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);

                        if (modelCustomizerDependenciesProperty == null)
                        {
                            throw new Exception("Unable to configure EF core model customizer for Onwrd");
                        }

                        var dependencies = modelCustomizerDependenciesProperty.GetValue(inner) as ModelCustomizerDependencies;

                        return new OnwrdModelCustomizer(inner, dependencies);
                    }

                    // Allow onward to wrap around original model customizer
                    services.Add(new ServiceDescriptor(
                        descriptor.ServiceType,
                        GetOnwrdModelCustomizer,
                        descriptor.Lifetime));
                }
            }

            public void Validate(IDbContextOptions options)
            {
            }
        }

        private class ExtensionInfo : DbContextOptionsExtensionInfo
        {
            private const string ExtensionName = "OnwrdDbContextOptionsExtension";

            public ExtensionInfo(IDbContextOptionsExtension extension)
                : base(extension)
            {
            }

            public override bool IsDatabaseProvider
                => false;

            public override string LogFragment => ExtensionName;

            public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo info)
                => true;

            public override int GetServiceProviderHashCode()
                => ExtensionName.GetHashCode();

            public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
            {
            }
        }
    }
}
