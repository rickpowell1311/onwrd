using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class OnConnectingInterceptor : DbConnectionInterceptor
    {
        private readonly RunOnce runOnce;
        private readonly OnwrdConfiguration configuration;
        private readonly IOnwrdMigrator migrator;

        public OnConnectingInterceptor(RunOnce runOnce, OnwrdConfiguration configuration, IOnwrdMigrator migrator)
        {
            this.runOnce = runOnce;
            this.configuration = configuration;
            this.migrator = migrator;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);

            if (this.configuration.RunMigrations)
            {
                await connection.OpenAsync();

                try
                {
                    await runOnce.ExecuteAsync("migrations", () => this.migrator.Migrate(connection));
                }
                finally
                {
                    await connection.CloseAsync();
                }
            }

            return result;
        }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            base.ConnectionOpening(connection, eventData, result);

            throw new NotSupportedException("Synchronous connections to the database are not supported with Onwrd");
        }
    }
}
