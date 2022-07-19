using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal static class ModelBuilderExtensions
    {
        internal static void AddOutboxingModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<OutboxMessage>(cfg =>
            {
                cfg.HasKey(x => x.Id).HasName("PK_Onwrd_Outbox");
                cfg.HasIndex(x => x.DispatchedOn, "IX_Onwrd_Outbox_DispatchedOn");

                cfg.ToTable("Outbox", "Onwrd");

                cfg.Property(x => x.Id)
                    .ValueGeneratedNever();
            });
        }
    }
}
