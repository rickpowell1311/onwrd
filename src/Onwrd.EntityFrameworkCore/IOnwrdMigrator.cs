using System.Data.Common;

namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwrdMigrator
    {
        Task Migrate(DbConnection connection);
    }
}
