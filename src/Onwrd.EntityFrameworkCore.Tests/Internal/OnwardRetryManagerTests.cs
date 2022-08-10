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

            onwardRetryConfiguration.PollPeriod = TimeSpan.FromSeconds(1);
            onwardRetryConfiguration.Attempts = 5;
            onwardRetryConfiguration.RetryAfter = TimeSpan.FromSeconds(3);

            this.wait = A.Fake<IWait>();
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenStopAfterNothingProcessedSetToTrue_DoesNotThrowTaskCanceledException()
        {
            this.onwardRetryConfiguration.StopWhenNothingProcessed = true;

            var sut = RetryManager();

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(TimeSpan.FromSeconds(30));

            await sut.RetryOnwardProcessing(cancellationTokenSource.Token);
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenEventsProcessedAndStopAfterNothingProcessedSetToTrue_ContinuesUntilNoMore()
        {
            this.onwardRetryConfiguration.StopWhenNothingProcessed = true;

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

            await sut.RetryOnwardProcessing(CancellationToken.None);

            Assert.Equal(numberOfEventsToProcess, processed);
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenNothingToProcess_WaitsForPollPeriod()
        {
            bool hasWaited = false;
            var cancellationTokenSource = new CancellationTokenSource();

            A.CallTo(() => this.onwardProcessingUnitOfWork.ProcessNext(A<CancellationToken>._))
                .ReturnsLazily(() =>
                {
                    if (!hasWaited)
                    {
                        hasWaited = true;
                        return UnitOfWorkResult.NoEvents;
                    }

                    cancellationTokenSource.Cancel();
                    return UnitOfWorkResult.NoEvents;
                });

            var sut = RetryManager();

            await Assert.ThrowsAsync<TaskCanceledException>(
                () => sut.RetryOnwardProcessing(cancellationTokenSource.Token));

            A.CallTo(() => wait.WaitFor(this.onwardRetryConfiguration.PollPeriod, A<CancellationToken>._))
                .MustHaveHappened();
        }

        [Fact]
        public async Task RetryOnwardProcessing_WhenRetriesExceeded_RetriesUpToRetryAttemptsWhenOnwardProcessingFails()
        {
            this.onwardRetryConfiguration.StopWhenNothingProcessed = true;

            A.CallTo(() => this.onwardProcessingUnitOfWork.ProcessNext(A<CancellationToken>._))
                .ThrowsAsync(new Exception("Oops"));

            var sut = RetryManager();

            await sut.RetryOnwardProcessing(CancellationToken.None);

            A.CallTo(() => wait.WaitFor(this.onwardRetryConfiguration.RetryAfter, A<CancellationToken>._))
                .MustHaveHappenedANumberOfTimesMatching(x => x == this.onwardRetryConfiguration.Attempts);
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
