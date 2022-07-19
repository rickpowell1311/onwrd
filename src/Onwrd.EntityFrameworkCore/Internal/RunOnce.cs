namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class RunOnce
    {
        internal HashSet<string> executedRunIds;

        public RunOnce()
        {
            this.executedRunIds = new HashSet<string>();
        }

        public async Task ExecuteAsync(string runId, Func<Task> action)
        {
            if (this.executedRunIds.Contains(runId))
            {
                return;
            }

            await action();

            this.executedRunIds.Add(runId);
        }

        public void Execute(string runId, Action action)
        {
            if (this.executedRunIds.Contains(runId))
            {
                return;
            }

            action();

            this.executedRunIds.Add(runId);
        }
    }
}
