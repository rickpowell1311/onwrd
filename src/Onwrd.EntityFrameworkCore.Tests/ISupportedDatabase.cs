using DotNet.Testcontainers.Containers;
using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Tests
{
    public interface ISupportedDatabase
    {
        TestcontainerDatabase TestcontainerDatabase { get; }

        void Configure(DbContextOptionsBuilder optionsBuilder);
    }
}
