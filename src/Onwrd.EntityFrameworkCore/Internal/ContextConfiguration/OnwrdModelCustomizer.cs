using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal class OnwrdModelCustomizer : ModelCustomizer
    {
        public OnwrdModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            switch (context.Database.ProviderName)
            {
                case "Microsoft.EntityFrameworkCore.SqlServer":
                case "Microsoft.EntityFrameworkCore.InMemory":
                    new SqlServerOnwrdModelBuilder().Build(modelBuilder);
                    break;
                case "Npgsql.EntityFrameworkCore.PostgreSQL":
                    new PostgreSqlOnwrdModelBuilder().Build(modelBuilder);
                    break;
                default: 
                    throw new NotSupportedException($"Provider '{context.Database.ProviderName}' is not supported");
            }
        }
    }
}
