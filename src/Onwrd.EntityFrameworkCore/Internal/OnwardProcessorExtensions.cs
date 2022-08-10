namespace Onwrd.EntityFrameworkCore.Internal
{
    internal static class OnwardProcessorExtensions
    {
        internal static async Task ProcessEvent(
            this IOnwardProcessor onwardProcessor, 
            (Event Event, object Contents) eventPair,
            CancellationToken cancellationToken = default)
        {
            var method = typeof(IOnwardProcessor).GetMethod("Process");

            var invocation = method
                .MakeGenericMethod(eventPair.Contents.GetType())
                .Invoke(onwardProcessor, new object[]
                {
                    eventPair.Contents,
                    new EventMetadata
                    {
                        Id = eventPair.Event.Id,
                        CreatedOn = eventPair.Event.CreatedOn
                    },
                    cancellationToken
                }) as Task;

            await invocation;
        }
    }
}
