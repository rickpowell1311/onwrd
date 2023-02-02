using System.Data.Common;

namespace Onwrd.EntityFrameworkCore
{
    public interface IOnwrdMigrator
    {
        Task MigrateAsync(DbConnection connection);

        void Migrate(DbConnection connection);
    }
}
