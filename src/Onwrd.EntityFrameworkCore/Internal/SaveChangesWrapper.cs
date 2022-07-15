using Microsoft.EntityFrameworkCore;
using Onwrd.EntityFrameworkCore.Internal.MessageExtraction;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class SaveChangesWrapper
    {
        private readonly DbContext context;
        private readonly Func<Task> saveChangesCallback;
        private readonly Func<IOnwardProcessor> onwardProcessorFactory;

        internal SaveChangesWrapper(
            DbContext context, 
            Func<Task> saveChangesCallback,
            Func<IOnwardProcessor> onwardProcessorFactory)
        {
            this.context = context;
            this.saveChangesCallback = saveChangesCallback;
            this.onwardProcessorFactory = onwardProcessorFactory;
        }

        public async Task SaveChangesAsync()
        {
            var messages = this.context.ExtractMessages();
            var additions = this.context.AddToOutbox(messages);

            await saveChangesCallback();

            var onwardProcessor = this.onwardProcessorFactory?.Invoke();

            if (onwardProcessor != null)
            {
                // TODO: May want to consider batched dispatch in the future
                foreach (var addition in additions)
                {
                    try
                    {
                        await onwardProcessor.ProcessMessage(addition);
                        addition.OutboxMessage.DispatchedOn = DateTime.UtcNow;

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
