using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class OnwardRetryManager<TContext> : IOnwardRetryManager<TContext>
        where TContext : DbContext
    {
        private readonly OnwrdRetryConfiguration configuration;
        private readonly IOnwardProcessingUnitOfWork<TContext> unitOfWork;
        private readonly IWait wait;

        public OnwardRetryManager(
            OnwrdRetryConfiguration configuration,
            IOnwardProcessingUnitOfWork<TContext> unitOfWork,
            IWait wait)
        {
            this.configuration = configuration;
            this.unitOfWork = unitOfWork;
            this.wait = wait;
        }

        public async Task RetryOnwardProcessing(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var retryUnitOfWorkResult = await ProcessWithRetries(cancellationToken);

                if (retryUnitOfWorkResult == RetryUnitOfWorkResult.Processed)
                {
                    continue;
                }

                if (this.configuration.StopWhenNothingProcessed)
                {
                    break;
                }

                await this.wait.WaitFor(this.configuration.PollPeriod, cancellationToken);

                continue;
            }
        }

        private async Task<RetryUnitOfWorkResult> ProcessWithRetries(CancellationToken cancellationToken)
        {
            var attempts = 0;

            while (attempts < configuration.Attempts)
            {
                try
                {
                    var unitOfWorkResult = await unitOfWork.ProcessNext(cancellationToken);

                    return unitOfWorkResult switch
                    {
                        UnitOfWorkResult.Processed => RetryUnitOfWorkResult.Processed,
                        UnitOfWorkResult.NoEvents => RetryUnitOfWorkResult.NoEvents,
                        _ => throw new NotImplementedException($"Cannot convert {nameof(UnitOfWorkResult)} '{unitOfWorkResult}' to {nameof(RetryUnitOfWorkResult)}"),
                    };
                }
                catch
                {
                    // TODO: Log
                    attempts++;
                    await this.wait.WaitFor(this.configuration.RetryAfter, cancellationToken);
                }
            }

            return RetryUnitOfWorkResult.RetriesExceeded;
        }
    }
}
