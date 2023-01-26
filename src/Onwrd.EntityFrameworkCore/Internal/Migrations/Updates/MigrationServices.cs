namespace Onwrd.EntityFrameworkCore.Internal.Migrations.Updates
{
    internal class MigrationServices
    {
        public IServiceProvider ServiceProvider { get; }

        public MigrationServices(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
        }
    }
}
