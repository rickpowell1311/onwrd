using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations
{
    [Migration("Onwrd_0001"), DbContext(typeof(MigrationContext))]
    internal class CreateSchema : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql<Updates.CreateSchema>();
        }
    }
}
