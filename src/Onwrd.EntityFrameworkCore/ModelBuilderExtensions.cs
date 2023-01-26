using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public static class ModelBuilderExtensions
    {
        /// <summary>
        /// Extends the client db context model with the Onwrd model configuration.
        /// </summary>
        /// <param name="modelBuilder"></param>
        public static void AddOnwrdModel(this ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(cfg =>
            {
                cfg.HasKey(x => x.Id);

                cfg.ToTable("Events", "Onwrd");

                cfg.Property(x => x.Id).ValueGeneratedNever();
            });
        }
    }
}
