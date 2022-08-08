using System.Collections.Generic;

namespace Onwrd.EntityFrameworkCore
{
    public abstract class EventRaiser
    {
        private readonly List<object> _events;

        public IEnumerable<object> Events => _events;

        protected EventRaiser()
        {
            _events = new List<object>();
        }

        protected void RaiseEvent<TEvent>(TEvent @event)
            where TEvent : class, new()
        {
            _events.Add(@event);
        }

        internal void ClearEvents()
        {
            _events.Clear();
        }
    }
}
