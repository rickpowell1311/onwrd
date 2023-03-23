using Onwrd.EntityFrameworkCore.Internal.Migrations;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal interface IOnwrdMigrator
    {
        IDatabaseMigrator GetDatabaseMigrator();
    }
}
