using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Text.Json.Serialization;

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

        internal static IEnumerable<(Event Event, object Contents)> AddToEvents(
            this DbContext context, 
            List<object> eventsContents)
        {
            var events = eventsContents
                .Select(x => new { Event = Event.FromContents(x), Contents = x })
                .ToList();

            context.AddRange(events.Select(x => x.Event));

            return events.Select(x => (x.Event, x.Contents));
        }
    }
}
