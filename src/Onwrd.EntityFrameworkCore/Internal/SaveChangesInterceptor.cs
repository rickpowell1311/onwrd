using Microsoft.EntityFrameworkCore.Diagnostics;
using Onwrd.EntityFrameworkCore.Internal.MessageExtraction;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class SaveChangesInterceptor : Microsoft.EntityFrameworkCore.Diagnostics.SaveChangesInterceptor
    {
        private readonly IOnwardProcessor onwardProcessor;

        public SaveChangesInterceptor(IOnwardProcessor onwardProcessor)
        {
            this.onwardProcessor = onwardProcessor;
        }

        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            /* In the synchronous calls to the database, adding messages to the outbox is supported, but onward processing isn't
             * because we don't know how the onward processor will behave and the implications involved in blocking calls
             */
            var messages = eventData.Context.ExtractMessages();
            eventData.Context.AddToOutbox(messages);

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            InterceptionResult<int> innerResult = default;

            var saveChangesWrapper = new SaveChangesWrapper(eventData.Context, async () =>
            {
                innerResult = await base.SavingChangesAsync(eventData, result, cancellationToken);
            }, this.onwardProcessor);

            await saveChangesWrapper.SaveChangesAsync();

            return innerResult;
        }
    }
}
