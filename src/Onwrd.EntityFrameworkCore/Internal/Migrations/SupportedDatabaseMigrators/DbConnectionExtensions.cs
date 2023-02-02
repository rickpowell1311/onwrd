using System.Data.Common;

namespace Onwrd.EntityFrameworkCore.Internal.Migrations.SupportedDatabaseMigrators
{
    internal static class DbConnectionExtensions
    {
        internal static void ExecuteSql(this DbConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            command.ExecuteNonQuery();
        }

        internal static async Task ExecuteSqlAsync(this DbConnection connection, string sql)
        {
            var command = connection.CreateCommand();
            command.CommandText = sql;
            await command.ExecuteNonQueryAsync();
        }
    }
}
