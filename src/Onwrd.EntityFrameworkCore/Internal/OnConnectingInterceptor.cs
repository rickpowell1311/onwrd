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

        public override async Task ConnectionOpenedAsync(DbConnection connection, ConnectionEndEventData eventData, CancellationToken cancellationToken = default)
        {
            await base.ConnectionOpenedAsync(connection, eventData, cancellationToken);

            await runOnce.ExecuteAsync("startup", startup.InitializeAsync);
        }

        public override void ConnectionOpened(DbConnection connection, ConnectionEndEventData eventData)
        {
            base.ConnectionOpened(connection, eventData);

            runOnce.Execute("startup", startup.Initialize);
        }
    }
}
