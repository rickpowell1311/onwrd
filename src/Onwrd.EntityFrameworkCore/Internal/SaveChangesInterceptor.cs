using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Onwrd.EntityFrameworkCore.Internal.EventExtraction;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class SaveChangesInterceptor<TContext> : SaveChangesInterceptor
        where TContext : DbContext
    {
        private readonly IOnwardProcessingUnitOfWork<TContext> unitOfWork;

        private readonly List<Guid> added;

        public SaveChangesInterceptor(
            IOnwardProcessor onwardProcessor,
            IOnwardProcessingUnitOfWork<TContext> unitOfWork)
        {
            this.unitOfWork = unitOfWork;

            this.added = new List<Guid>();
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData, 
            InterceptionResult<int> result)
        {
            var events = eventData.Context.ExtractEvents();
            var added = eventData.Context.AddToEvents(events);

            this.added.AddRange(added.Select(x => x.Id));

            return base.SavingChanges(eventData, result);
        }

        public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData, 
            InterceptionResult<int> result, 
            CancellationToken cancellationToken = default)
        {
            var context = eventData.Context;

            var events = context.ExtractEvents();
            var added = eventData.Context.AddToEvents(events);

            this.added.AddRange(added.Select(x => x.Id));

            return await base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public override async ValueTask<int> SavedChangesAsync(
            SaveChangesCompletedEventData eventData, 
            int result, CancellationToken 
            cancellationToken = default)
        {
            var baseResult = await base.SavedChangesAsync(eventData, result, cancellationToken);

            while (added.TryPop(out var addition))
            {
                try
                {
                    await this.unitOfWork.ProcessEvent(addition, cancellationToken);
                }
                catch (Exception ex)
                {
                    // TODO: Log
                }
            }

            return baseResult;
        }
    }
}
