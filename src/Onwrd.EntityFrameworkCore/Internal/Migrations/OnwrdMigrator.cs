using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal class OnwrdMigrator : IOnwrdMigrator
    {
        private readonly IServiceProvider serviceProvider;
        private readonly Func<IServiceProvider, DbContext> contextFactory;

        public OnwrdMigrator(IServiceProvider serviceProvider, Func<IServiceProvider, DbContext> contextFactory)
        {
            this.serviceProvider = serviceProvider;
            this.contextFactory = contextFactory;
        }

        public IDatabaseMigrator GetDatabaseMigrator()
        {
            return GetDbProvider() switch
            {
                "Microsoft.EntityFrameworkCore.SqlServer" => new SqlServerDatabaseMigrator(),
                "Npgsql.EntityFrameworkCore.PostgreSQL" => new PostgreSqlDatabaseMigrator(),
                _ => throw new NotImplementedException("Database provider not supported"),
            };
        }

        private string GetDbProvider()
        {
            using var scope = serviceProvider.CreateScope();
            using var context = contextFactory(scope.ServiceProvider);
            return context.Database.ProviderName;
        }
    }
}
