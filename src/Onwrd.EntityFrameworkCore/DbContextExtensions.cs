using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.EventExtraction;

namespace Onwrd.EntityFrameworkCore
{
    public static class DbContextExtensions
    {
        public static void RaiseEvent<T>(this DbContext dbContext, T @event)
        {
            dbContext.AddToEvents(new List<object> { @event });
        }
    }
}
