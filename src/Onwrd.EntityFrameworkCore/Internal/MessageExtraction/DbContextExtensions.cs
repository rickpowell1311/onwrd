using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Onwrd.EntityFrameworkCore.Internal.MessageExtraction
{
    internal static class DbContextExtensions
    {
        internal static List<object> ExtractMessages(this DbContext context)
        {
            var messages = new List<object>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                messages.AddRange(entry.ExtractMessages());
            }

            return messages;
        }

        internal static IEnumerable<(OutboxMessage OutboxMessage, object Message)> AddToOutbox(this DbContext context, List<object> messages)
        {
            var outboxMessages = messages
                .Select(x => new { OutboxMessage = OutboxMessage.FromMessage(x), Message = x })
                .ToList();

            context.AddRange(outboxMessages.Select(x => x.OutboxMessage));

            return outboxMessages.Select(x => (x.OutboxMessage, x.Message));
        }
    }
}
