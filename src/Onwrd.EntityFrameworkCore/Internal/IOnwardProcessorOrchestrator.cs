using Microsoft.Extensions.DependencyInjection;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal interface IOnwardProcessorOrchestrator
    {
        Task Process(
            (Event Event, object Contents) eventPair,
            IServiceScope scope,
            CancellationToken cancellationToken);
    }
}
