using System.Text.Json;

namespace Onwrd.EntityFrameworkCore
{
    internal class Event
    {
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? DispatchedOn { get; set; }

        public string TypeId { get; set; }

        public string Contents { get; set; }

        internal static Event FromContents(object contents)
        {
            return new Event
            {
                TypeId = contents.GetType().FullName,
                Contents = JsonSerializer.Serialize(contents),
                CreatedOn = DateTime.UtcNow,
                DispatchedOn = null,
                Id = Guid.NewGuid()
            };
        }
    }
}