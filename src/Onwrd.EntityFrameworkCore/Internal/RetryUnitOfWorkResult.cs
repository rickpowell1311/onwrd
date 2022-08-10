namespace Onwrd.EntityFrameworkCore.Internal
{
    internal enum RetryUnitOfWorkResult
    {
        Processed,
        NoEvents,
        RetriesExceeded
    }
}
