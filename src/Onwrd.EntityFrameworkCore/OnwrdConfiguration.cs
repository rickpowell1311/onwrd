using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdConfiguration
    {
        internal Type OnwardProcessorType { get; private set; }

        internal OnwrdRetryConfiguration RetryConfiguration { get; private set; }

        public bool RunMigrations { get; set; } = true;

        internal OnwrdConfiguration()
        {
            RetryConfiguration = new OnwrdRetryConfiguration();
            OnwardProcessorType = typeof(NoOpOnwardProcessor);
        }

        public void ConfigureRetryOptions(Action<OnwrdRetryConfiguration> retryConfig)
        {
            retryConfig(RetryConfiguration);
        }

        public void UseOnwardProcessor<T>()
            where T : class, IOnwardProcessor
        {
            OnwardProcessorType = typeof(T);
        }
    }
}
