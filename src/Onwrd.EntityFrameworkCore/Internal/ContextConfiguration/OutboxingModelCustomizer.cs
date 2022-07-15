using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Onwrd.EntityFrameworkCore.Internal.ContextConfiguration
{
    internal class OutboxingModelCustomizer : ModelCustomizer
    {
        public OutboxingModelCustomizer(ModelCustomizerDependencies dependencies) : base(dependencies)
        {
        }

        public override void Customize(ModelBuilder modelBuilder, DbContext context)
        {
            base.Customize(modelBuilder, context);

            modelBuilder.AddOutboxingModel();
        }
    }
}
