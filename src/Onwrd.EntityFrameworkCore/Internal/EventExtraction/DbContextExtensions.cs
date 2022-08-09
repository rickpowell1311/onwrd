using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.EventExtraction
{
    internal static class DbContextExtensions
    {
        internal static List<object> ExtractEvents(this DbContext context)
        {
            var events = new List<object>();

            foreach (var entry in context.ChangeTracker.Entries())
            {
                events.AddRange(entry.ExtractEvents());
            }

            return events;
        }

        internal static IEnumerable<Event> AddToEvents(
            this DbContext context, 
            List<object> eventsContents)
        {
            var events = eventsContents
                .Select(x => Event.FromContents(x))
                .ToList();

            context.AddRange(events);

            return events;
        }
    }
}
