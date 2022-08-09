namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardRetryManager
    {
        Task RetryOnwardProcessing(CancellationToken cancellationToken);
    }
}
