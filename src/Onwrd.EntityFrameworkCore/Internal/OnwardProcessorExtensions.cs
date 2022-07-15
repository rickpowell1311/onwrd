namespace Onwrd.EntityFrameworkCore.Internal
{
    internal static class OnwardProcessorExtensions
    {
        internal static async Task ProcessMessage(
            this IOnwardProcessor onwardProcessor, 
            (OutboxMessage OutboxMessage, object Message) messagePair)
        {
            var method = typeof(IOnwardProcessor).GetMethod("Process");

            var invocation = method
                .MakeGenericMethod(messagePair.Message.GetType())
                .Invoke(onwardProcessor, new object[]
                {
                    messagePair.Message,
                    new MessageMetadata
                    {
                        Id = messagePair.OutboxMessage.Id,
                        CreatedOn = messagePair.OutboxMessage.CreatedOn
                    }
                }) as Task;

            await invocation;
        }
    }
}
