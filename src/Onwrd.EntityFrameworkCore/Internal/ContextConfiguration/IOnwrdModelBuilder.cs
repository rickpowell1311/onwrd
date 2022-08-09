using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal interface IOnwrdModelbuilder
    {
        void Build(ModelBuilder modelBuilder);
    }
}
