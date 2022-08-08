using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.EventExtraction;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class SaveChangesWrapper
    {
        private readonly DbContext context;
        private readonly Func<Task> saveChangesCallback;
        private readonly IOnwardProcessor onwardProcessor;

        internal SaveChangesWrapper(
            DbContext context, 
            Func<Task> saveChangesCallback,
            IOnwardProcessor onwardProcessor)
        {
            this.context = context;
            this.saveChangesCallback = saveChangesCallback;
            this.onwardProcessor = onwardProcessor;
        }

        public async Task SaveChangesAsync()
        {
            var events = this.context.ExtractEvents();
            var additions = this.context.AddToEvents(events);

            await saveChangesCallback();

            if (onwardProcessor != null)
            {
                // TODO: May want to consider batched dispatch in the future
                foreach (var addition in additions)
                {
                    try
                    {
                        await onwardProcessor.ProcessEvent(addition);
                        addition.Event.DispatchedOn = DateTime.UtcNow;

                        await context.SaveChangesAsync();
                    }
                    catch
                    {
                        // TODO: Add Logging
                    }
                }
            }
        }
    }
}
