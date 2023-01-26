namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardProcessor
    {
        Task Process<T>(T @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default);
    }

    public interface IOnwardProcessor<T>
    {
        Task Process(T @event, EventMetadata eventMetadata, CancellationToken cancellationToken = default);
    }
}
