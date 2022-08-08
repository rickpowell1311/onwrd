using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdConfiguration
    {
        internal Type OnwardProcessorType { get; private set; }

        public bool RunMigrations { get; set; } = true;

        internal OnwrdConfiguration()
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
