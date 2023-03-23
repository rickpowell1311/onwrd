using Onwrd.EntityFrameworkCore.Internal.Migrations;

namespace Onwrd.EntityFrameworkCore
{
    internal interface IOnwrdMigrator
    {
        IDatabaseMigrator GetDatabaseMigrator();
    }
}
