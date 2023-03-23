using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal interface IDatabaseMigrator
    {
        Task MigrateAsync(DbConnection connection);

        void Migrate(DbConnection connection);
    }
}
