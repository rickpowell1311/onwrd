namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdRetryConfiguration
    {
        public int Attempts { get; set; } = 5;

        public TimeSpan RetryAfter = TimeSpan.FromSeconds(5);

        public TimeSpan PollPeriod { get; set; } = TimeSpan.FromSeconds(5);

        internal bool StopWhenNothingProcessed { get; set; } = false;

        internal OnwrdRetryConfiguration()
        {
        }
    }
}
