using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    [Migration("Onwrd_0001_Create"), DbContext(typeof(MigrationContext))]
    internal class Create : Migration, IOnwrdMigration
    {
        public MigrationContext Context { get; set; }

        protected override void Up(MigrationBuilder _)
        {
            if (Context.Database.IsRelational())
            {
                var databaseCreator = Context.GetService<IRelationalDatabaseCreator>();
                databaseCreator.CreateTables();
            }
        }
    }
}
