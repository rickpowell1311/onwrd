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

        public async Task<RetryResult> RetryOnwardProcessing(CancellationToken cancellationToken)
        {
            var eventsProcessed = 0;

            while (!cancellationToken.IsCancellationRequested)
            {
                var retryUnitOfWorkResult = await ProcessWithRetries(cancellationToken);

                if (retryUnitOfWorkResult is UnsuccessfulRetryResult)
                {
                    return retryUnitOfWorkResult;
                }

                if (retryUnitOfWorkResult is SuccessfulRetryResult successfulRetryResult 
                    && successfulRetryResult.NumberOfEventsProcessed > 0)
                {
                    eventsProcessed++;
                    continue;
                }

                break;
            }

            return new SuccessfulRetryResult(eventsProcessed);
        }

        private async Task<RetryResult> ProcessWithRetries(CancellationToken cancellationToken)
        {
            var attempts = 0;
            Exception latestException = default;

            while (attempts < configuration.MaximumRetryAttempts)
            {
                try
                {
                    var unitOfWorkResult = await unitOfWork.ProcessNext(cancellationToken);

                    return unitOfWorkResult switch
                    {
                        UnitOfWorkResult.Processed => new SuccessfulRetryResult(1),
                        UnitOfWorkResult.NoEvents => new SuccessfulRetryResult(0),
                        _ => throw new NotImplementedException($"Cannot convert {nameof(UnitOfWorkResult)} '{unitOfWorkResult}' to {nameof(RetryResult)}"),
                    };
                }
                catch (Exception ex)
                {
                    attempts++;
                    latestException = ex;

                    if (attempts == configuration.MaximumRetryAttempts)
                    {
                        continue;
                    }

                    await this.wait.WaitFor(this.configuration.RetryAfter, cancellationToken);
                }
            }

            return new UnsuccessfulRetryResult(attempts, latestException);
        }
    }
}
