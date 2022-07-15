using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace Onwrd.EntityFrameworkCore.Internal.MessageExtraction
{
    internal static class EntityEntryExtensions
    {
        internal static List<object> ExtractMessages(this EntityEntry entityEntry)
        {
            var messages = new List<object>();

            if (entityEntry.Entity is Outboxed outboxed)
            {
                messages.AddRange(outboxed.Messages);
                outboxed.ClearMessages();
            }

            return messages;
        }
    }
}
