namespace Onwrd.EntityFrameworkCore
{
    public abstract class RetryResult
    {
        public bool IsSuccess { get; protected set; }

        protected RetryResult()
        {
        }
    }

    public class SuccessfulRetryResult : RetryResult
    {
        public int NumberOfEventsProcessed { get; }

        internal SuccessfulRetryResult(int numberOfEventsProcessed)
        {
            this.IsSuccess = true;
            this.NumberOfEventsProcessed = numberOfEventsProcessed;
        }
    }

    public class UnsuccessfulRetryResult : RetryResult
    {
        public int NumberOfRetries { get; }

        public Exception LastException { get; }

        internal UnsuccessfulRetryResult(int numberOfRetries, Exception lastException)
        {
            this.NumberOfRetries = numberOfRetries;
            this.LastException = lastException;
            this.IsSuccess = false;
        }
    }
}
