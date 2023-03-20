using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Onwrd.EntityFrameworkCore;

namespace Onwrd.Extensions.Hosting
{
    public class OnwrdBackgroundRetries<TContext> : BackgroundService
        where TContext : DbContext 
    {
        public OnwrdBackgroundRetries(IServiceProvider services)
        {
            Services = services;
        }

        public IServiceProvider Services { get; }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await DoWork(stoppingToken);
        }

        private async Task DoWork(CancellationToken stoppingToken)
        {
            using (var scope = Services.CreateScope())
            {
                var scopedProcessingService =
                    scope.ServiceProvider
                        .GetRequiredService<OnwrdBackgroundRetriesService>();

                await scopedProcessingService.DoWork(stoppingToken);
            }
        }

        public override async Task StopAsync(CancellationToken stoppingToken)
        {
            await base.StopAsync(stoppingToken);
        }

        public class OnwrdBackgroundRetriesService
        {
            private readonly IOnwardRetryManager<TContext> _onwrdRetryManager;
            private readonly ILogger<OnwrdBackgroundRetriesService> _logger;
            private readonly TimeSpan _unsuccessfulRetryPeriod = TimeSpan.FromSeconds(30);
            private readonly TimeSpan _successfulRetryPeriod = TimeSpan.FromMinutes(10);

            public OnwrdBackgroundRetriesService(
                IOnwardRetryManager<TContext> onwrdRetryManager,
                ILogger<OnwrdBackgroundRetriesService> logger)
            {
                _onwrdRetryManager = onwrdRetryManager;
                _logger = logger;
            }

            public async Task DoWork(CancellationToken stoppingToken)
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Running retries for Onwrd events");

                    var result = await _onwrdRetryManager.RetryOnwardProcessing(stoppingToken);

                    if (!result.IsSuccess)
                    {
                        var failureResult = result as UnsuccessfulRetryResult;

                        _logger.LogWarning(
                            failureResult.LastException,
                            "Some onwrd events could not be processed. Look at the inner exception for more details on the most recent failure");
                        await Task.Delay(_unsuccessfulRetryPeriod, stoppingToken);
                    }
                    else
                    {
                        var successResult = result as SuccessfulRetryResult;

                        _logger.LogInformation("Successfully completed retries of onwrd events. {0} events were processed", successResult.NumberOfEventsProcessed);
                        await Task.Delay(_successfulRetryPeriod, stoppingToken);
                    }
                }
            }
        }
    }
}
