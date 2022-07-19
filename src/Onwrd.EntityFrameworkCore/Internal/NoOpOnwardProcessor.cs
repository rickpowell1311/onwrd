namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class NoOpOnwardProcessor : IOnwardProcessor
    {
        public Task Process<T>(T message, MessageMetadata messageMetadata)
        {
            return Task.CompletedTask;
        }
    }
}
