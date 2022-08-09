namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class Wait : IWait
    {
        public Task WaitFor(TimeSpan timeout, CancellationToken cancellationToken = default)
        {
            return Task.Delay(timeout, cancellationToken);
        }
    }
}
