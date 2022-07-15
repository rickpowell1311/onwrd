using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal static class ModelBuilderExtensions
    {
        internal static void AddOutboxingModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboxMessage>(cfg =>
            {
                cfg.HasKey(x => x.Id);
            });
        }
    }
}
