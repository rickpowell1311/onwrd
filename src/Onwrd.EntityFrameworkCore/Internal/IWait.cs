namespace Onwrd.EntityFrameworkCore.Internal
{
    internal interface IWait
    {
        Task WaitFor(TimeSpan timeout, CancellationToken cancellationToken = default);
    }
}
