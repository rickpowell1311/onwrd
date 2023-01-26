using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class OnwardProcessingUnitOfWork<TContext> : IOnwardProcessingUnitOfWork<TContext>
        where TContext : DbContext
    {
        private readonly IServiceProvider serviceProvider;
        private readonly IOnwardProcessorOrchestrator onwardProcessorOrchestrator;

        public OnwardProcessingUnitOfWork(
            IServiceProvider serviceProvider,
            IOnwardProcessorOrchestrator onwardProcessorOrchestrator)
        {
            this.serviceProvider = serviceProvider;
            this.onwardProcessorOrchestrator = onwardProcessorOrchestrator;
        }

        public async Task<UnitOfWorkResult> ProcessEvent(Guid eventId, CancellationToken cancellationToken)
        {
            async Task<Event> getEvent(TContext context, CancellationToken cancellationToken)
            {
                var @event = await context
                    .Set<Event>()
                    .FirstOrDefaultAsync(x => x.Id == eventId, cancellationToken);

                return @event;
            }

            return await Process(getEvent, cancellationToken);
        }

        public async Task<UnitOfWorkResult> ProcessNext(CancellationToken cancellationToken)
        {
            static async Task<Event> getNext(TContext context, CancellationToken cancellationToken)
            {
                var next = await context.Set<Event>()
                    .Where(x => !x.DispatchedOn.HasValue)
                    .OrderBy(x => x.CreatedOn)
                    .FirstOrDefaultAsync(cancellationToken: cancellationToken);

                if (next == null)
                {
                    return null;
                }

                return next;
            }

            return await Process(getNext, cancellationToken);
        }

        private async Task<UnitOfWorkResult> Process(
            Func<TContext, CancellationToken, Task<Event>> getEvent,
            CancellationToken cancellationToken)
        {
            using var scope = this.serviceProvider.CreateScope();
            var serviceProvider = scope.ServiceProvider;
            using var context = serviceProvider.GetService<TContext>();

            var @event = await getEvent(context, cancellationToken);

            if (@event == null)
            {
                return UnitOfWorkResult.NoEvents;
            }

            var contents = @event.DeserializeContents();

            await onwardProcessorOrchestrator.Process((@event, contents), scope, cancellationToken);

            @event.DispatchedOn = DateTime.UtcNow;

            await context.SaveChangesAsync(cancellationToken);

            return UnitOfWorkResult.Processed;
        }
    }
}
