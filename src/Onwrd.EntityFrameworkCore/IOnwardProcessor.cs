namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwardProcessor
    {
        Task Process<T>(T message, MessageMetadata messageMetadata);
    }
}
