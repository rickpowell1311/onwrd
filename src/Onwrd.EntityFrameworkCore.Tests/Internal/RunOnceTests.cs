using Onwrd.EntityFrameworkCore.Internal;
using Xunit;

namespace Onwrd.EntityFrameworkCore.Tests.Internal
{
    public class RunOnceTests
    {
        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Execute_WhenCalledOnce_RunsOnce(bool isAsync)
        {
            var runInterations = 0;
            var runOnce = new RunOnce();

            if (isAsync)
            {
                await runOnce.ExecuteAsync("test", () =>
                {
                    runInterations++;
                    return Task.CompletedTask;
                });
            }
            else
            {
                runOnce.Execute("test", () => runInterations++);
            }            

            Assert.Equal(1, runInterations);
        }

        [Theory]
        [InlineData(2, true)]
        [InlineData(2, false)]
        [InlineData(3, true)]
        [InlineData(3, false)]
        public async Task Execute_WhenCalledMultipleTimes_RunsOnce(int runs, bool isAsync)
        {
            var runInterations = 0;
            var runOnce = new RunOnce();

            if (isAsync)
            {
                for (int i = 0; i < runs; i++)
                {
                    await runOnce.ExecuteAsync("test", () =>
                    {
                        runInterations++;
                        return Task.CompletedTask;
                    });
                }
            }
            else
            {
                for (int i = 0; i < runs; i++)
                {
                    runOnce.Execute("test", () => runInterations++);
                }
            }

            Assert.Equal(1, runInterations);
        }
    }
}
