using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public class OutboxingConfiguration
    {
        internal Type OnwardProcessorType { get; private set; }

        public bool RunMigrations { get; set; } = true;

        internal OutboxingConfiguration()
        {
            OnwardProcessorType = typeof(NoOpOnwardProcessor);
        }

        public void UseOnwardProcessor<T>()
            where T : class, IOnwardProcessor
        {
            OnwardProcessorType = typeof(T);
        }
    }
}
