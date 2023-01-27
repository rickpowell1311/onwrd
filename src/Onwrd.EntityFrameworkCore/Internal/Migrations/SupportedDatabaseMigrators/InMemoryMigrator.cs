using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal class InMemoryMigrator
    {
        public Task Migrate(DbConnection connection)
        {
            return Task.CompletedTask;
        }
    }
}
