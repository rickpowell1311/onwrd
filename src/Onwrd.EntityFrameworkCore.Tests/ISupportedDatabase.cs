using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public interface ISupportedDatabase
    {
        TestcontainerDatabase TestcontainerDatabase { get; }

        public void Configure(DbContextOptionsBuilder optionsBuilder);
    }
}
