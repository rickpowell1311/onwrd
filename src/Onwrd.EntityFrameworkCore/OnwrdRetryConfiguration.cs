namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdRetryConfiguration
    {
        public int MaximumRetryAttempts { get; set; } = 5;

        public TimeSpan RetryAfter = TimeSpan.FromSeconds(5);

        internal OnwrdRetryConfiguration()
        {
        }
    }
}
