namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardProcessor
    {
        Task Process<T>(T @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default);
    }
}
