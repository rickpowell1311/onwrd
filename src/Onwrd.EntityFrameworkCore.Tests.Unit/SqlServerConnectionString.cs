namespace Onwrd.EntityFrameworkCore.Tests.Unit
{
    internal static class SqlServerConnectionString
    {
        internal static string ForDatabase(string databaseName)
        {
            var server = Server();
            var creds = Credentials();

            return $"Server={server};Database={databaseName};{creds};";
        }

        private static string Server()
        {
            var server = Environment.GetEnvironmentVariable("DATABASE_SERVER");

            if (!string.IsNullOrWhiteSpace(server))
            {
                return server;
            }

            return "."; // default to local server if no environment variable override
        }

        private static string Credentials()
        {
            var username = Environment.GetEnvironmentVariable("DATABASE_USER");
            var password = Environment.GetEnvironmentVariable("DATABASE_PASSWORD");

            if (!string.IsNullOrWhiteSpace(username) && !string.IsNullOrWhiteSpace(password))
            {
                return $"User={username};Password={password}";
            }

            return "Trusted_Connection=True"; // default to local server if no environment variable override
        }
    }
}
