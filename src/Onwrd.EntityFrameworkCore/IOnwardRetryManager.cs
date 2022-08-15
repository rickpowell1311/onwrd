using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardRetryManager<TContext>
        where TContext : DbContext
    {
        /// <summary>
        /// Retries unprocessed events in the order that they occurred.
        /// A RetryResult indicates whether the Retry operation was a success, and can either be a SuccessfulRetryResult or 
        /// an UnsuccessfulRetryResult. Attempting casts to either can give further details
        /// </summary>
        /// <param name="cancellationToken"></param>
        /// <returns>RetryResult</returns>
        Task<RetryResult> RetryOnwardProcessing(CancellationToken cancellationToken);
    }
}
