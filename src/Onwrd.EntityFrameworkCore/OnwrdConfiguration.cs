using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdConfiguration
    {
        internal Type OnwardProcessorType { get; private set; }

        internal OnwrdRetryConfiguration RetryConfiguration { get; private set; }

        internal OnwrdProcessorsConfiguration OnwrdProcessorsConfiguration { get; private set; }

        public bool RunMigrations { get; set; } = true;

        public bool UseOnwrdProcessor { get; private set; }

        public bool UseOnwrdProcessors { get; private set; }

        internal OnwrdConfiguration()
        {
            RetryConfiguration = new OnwrdRetryConfiguration();
            OnwardProcessorType = typeof(NoOpOnwardProcessor);
            OnwrdProcessorsConfiguration = new OnwrdProcessorsConfiguration();
        }

        public void ConfigureRetryOptions(Action<OnwrdRetryConfiguration> retryConfig)
        {
            retryConfig(RetryConfiguration);
        }

        public void UseOnwardProcessor<T>()
            where T : class, IOnwardProcessor
        {
            OnwardProcessorType = typeof(T);

            UseOnwrdProcessor = true;
            UseOnwrdProcessors = false;
        }

        public void UseOnwardProcessors(Action<OnwrdProcessorsConfiguration> onwrdProcessorsConfig)
        {
            onwrdProcessorsConfig(OnwrdProcessorsConfiguration);

            UseOnwrdProcessor = false;
            UseOnwrdProcessors = true;
        }
    }
}
