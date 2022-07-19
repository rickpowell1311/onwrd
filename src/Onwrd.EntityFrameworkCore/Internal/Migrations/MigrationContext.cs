using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations.Schema;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal class MigrationContext : DbContext
    {
        public MigrationContext(DbContextOptions options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasDefaultSchema("Onwrd");

            base.OnModelCreating(modelBuilder);
        }
    }
}
