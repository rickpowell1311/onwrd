namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class NoOpOnwardProcessor : IOnwardProcessor
    {
        public Task Process<T>(T @event, EventMetadata eventMetadata)
        {
            return Task.CompletedTask;
        }
    }
}
