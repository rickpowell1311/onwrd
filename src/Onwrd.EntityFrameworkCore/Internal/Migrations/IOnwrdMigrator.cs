using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    internal interface IOnwrdMigrator
    {
        Task Migrate(DbConnection connection);
    }
}
