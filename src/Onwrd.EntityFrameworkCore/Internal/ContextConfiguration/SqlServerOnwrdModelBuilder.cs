using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal class SqlServerOnwrdModelBuilder : IOnwrdModelbuilder
    {
        public void Build(ModelBuilder modelBuilder)
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
