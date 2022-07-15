using System.Text.Json;

namespace Onwrd.EntityFrameworkCore
{
    internal class OutboxMessage
    {
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? DispatchedOn { get; set; }

        public string TypeId { get; set; }

        public string Contents { get; set; }

        internal static OutboxMessage FromMessage(object message)
        {
            return new OutboxMessage
            {
                TypeId = message.GetType().FullName,
                Contents = JsonSerializer.Serialize(message),
                CreatedOn = DateTime.UtcNow,
                DispatchedOn = null,
                Id = Guid.NewGuid()
            };
        }
    }
}