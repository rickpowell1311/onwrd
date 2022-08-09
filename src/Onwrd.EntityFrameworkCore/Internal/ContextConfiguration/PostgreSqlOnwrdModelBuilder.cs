using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal class PostgreSqlOnwrdModelBuilder : IOnwrdModelbuilder
    {
        public void Build(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Event>(cfg =>
            {
                cfg.ToTable("events", "onwrd");

                cfg.HasKey(x => x.Id);

                cfg.Property(x => x.Id)
                    .HasColumnName("id")
                    .ValueGeneratedNever();

                cfg.Property(x => x.Contents).HasColumnName("contents");
                cfg.Property(x => x.CreatedOn).HasColumnName("created_on");
                cfg.Property(x => x.DispatchedOn).HasColumnName("dispatched_on");
                cfg.Property(x => x.TypeId).HasColumnName("type_id");
            });
        }
    }
}
