using FakeItEasy;
using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class OnwardRetryManagerTests
    {
        private readonly OnwrdRetryConfiguration onwardRetryConfiguration;
        private readonly IOnwardProcessingUnitOfWork<TestContext> onwardProcessingUnitOfWork;
        private readonly IWait wait;

        public OnwardRetryManagerTests()
        {
            this.onwardRetryConfiguration = new OnwrdRetryConfiguration();

            this.onwardProcessingUnitOfWork = A.Fake<IOnwardProcessingUnitOfWork<TestContext>>();

            // No events to process by default
            A.CallTo(() => this.onwardProcessingUnitOfWork.ProcessNext(A<CancellationToken>._))
                .Returns(UnitOfWorkResult.NoEvents);

            onwardRetryConfiguration.MaximumRetryAttempts = 5;
            onwardRetryConfiguration.RetryAfter = TimeSpan.FromSeconds(3);

            this.wait = A.Fake<IWait>();
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenProcessingCompleteBeforeCancellation_DoesNotThrowTaskCanceledException()
        {
            var sut = RetryManager();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            await sut.RetryOnwardProcessing(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenEventsProcessed_ContinuesUntilNoMore()
        {
            var processed = 0;
            var numberOfEventsToProcess = 10;

            A.CallTo(() => this.onwardProcessingUnitOfWork.ProcessNext(A<CancellationToken>._))
                .ReturnsLazily(() =>
                {
                    if (processed < numberOfEventsToProcess)
                    {
                        processed++;
                        return UnitOfWorkResult.Processed;
                    }

                    return UnitOfWorkResult.NoEvents;
                });

            var sut = RetryManager();

            var result = await sut.RetryOnwardProcessing(CancellationToken.None);

            Assert.Equal(numberOfEventsToProcess, processed);
            Assert.True(result.IsSuccess);
            Assert.IsType<SuccessfulRetryResult>(result);
            Assert.Equal(numberOfEventsToProcess, ((SuccessfulRetryResult)result).NumberOfEventsProcessed);
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenRetriesExceeded_RetriesUpToRetryAttemptsWhenOnwardProcessingFails()
        {
            A.CallTo(() => this.onwardProcessingUnitOfWork.ProcessNext(A<CancellationToken>._))
                .ThrowsAsync(new Exception("Oops"));

            var sut = RetryManager();

            var result = await sut.RetryOnwardProcessing(CancellationToken.None);

            /* When retries have been exceeded, we shouldn't wait again, so expected waits is Attempts - 1 
                e.g. for 3 failed attempts, the execution should be:
                    - Attempt 1
                    - Wait
                    - Attempt 2
                    - Wait
                    - Attempt 3
             */
            var expectedNumberOfWaits = this.onwardRetryConfiguration.MaximumRetryAttempts - 1;

            A.CallTo(() => wait.WaitFor(this.onwardRetryConfiguration.RetryAfter, A<CancellationToken>._))
                .MustHaveHappenedANumberOfTimesMatching(x => x == expectedNumberOfWaits);

            Assert.False(result.IsSuccess);
            Assert.IsType<UnsuccessfulRetryResult>(result);

            var unsuccessfulRetryResult = (UnsuccessfulRetryResult)result;
            Assert.Equal("Oops", unsuccessfulRetryResult.LastException.Message);
            Assert.Equal(unsuccessfulRetryResult.NumberOfRetries, this.onwardRetryConfiguration.MaximumRetryAttempts);
        }

        private OnwardRetryManager<TestContext> RetryManager()
        {
            return new OnwardRetryManager<TestContext>(
                this.onwardRetryConfiguration,
                this.onwardProcessingUnitOfWork,
                this.wait);
        }

        internal class TestContext : DbContext
        {
            public TestContext(DbContextOptions<TestContext> options) : base(options)
            {
            }
        }
    }
}
