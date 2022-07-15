using System.Collections.Generic;

namespace Onwrd.EntityFrameworkCore
{
    public abstract class Outboxed
    {
        private readonly List<object> _messages;

        public IEnumerable<object> Messages => _messages;

        protected Outboxed()
        {
            _messages = new List<object>();
        }

        protected void AddToOutbox<TMessage>(TMessage message)
            where TMessage : class, new()
        {
            _messages.Add(message);
        }

        internal void ClearMessages()
        {
            _messages.Clear();
        }
    }
}
