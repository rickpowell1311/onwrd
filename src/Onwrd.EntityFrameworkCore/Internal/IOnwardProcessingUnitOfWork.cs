using Microsoft.EntityFrameworkCore;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal interface IOnwardProcessingUnitOfWork<TContext>
        where TContext : DbContext
    {
        Task<UnitOfWorkResult> ProcessEvent(Guid eventId, CancellationToken cancellationToken);

        Task<UnitOfWorkResult> ProcessNext(CancellationToken cancellationToken);
    }
}
