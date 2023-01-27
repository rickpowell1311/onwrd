using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class OnConnectingInterceptor : DbConnectionInterceptor
    {
        private readonly RunOnce runOnce;
        private readonly Startup startup;

        public OnConnectingInterceptor(RunOnce runOnce, Startup startup)
        {
            this.runOnce = runOnce;
            this.startup = startup;
        }

        public override async ValueTask<InterceptionResult> ConnectionOpeningAsync(DbConnection connection, ConnectionEventData eventData, InterceptionResult result, CancellationToken cancellationToken = default)
        {
            await base.ConnectionOpeningAsync(connection, eventData, result, cancellationToken);

            await runOnce.ExecuteAsync("startup", startup.InitializeAsync);

            return result;
        }

        public override InterceptionResult ConnectionOpening(DbConnection connection, ConnectionEventData eventData, InterceptionResult result)
        {
            base.ConnectionOpening(connection, eventData, result);

            runOnce.Execute("startup", startup.Initialize);

            return result;
        }
    }
}
