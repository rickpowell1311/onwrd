using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal static class ModelBuilderExtensions
    {
        internal static void AddOnwrdModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(cfg =>
            {
                cfg.HasKey(x => x.Id).HasName("PK_Onwrd_Event");
                cfg.HasIndex(x => x.DispatchedOn, "IX_Onwrd_Events_DispatchedOn");

                cfg.ToTable("Events", "Onwrd");

                cfg.Property(x => x.Id).ValueGeneratedNever();
            });
        }
    }
}
