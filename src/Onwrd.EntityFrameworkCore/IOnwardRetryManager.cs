using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardRetryManager<TContext>
        where TContext : DbContext
    {
        Task RetryOnwardProcessing(CancellationToken cancellationToken);
    }
}
