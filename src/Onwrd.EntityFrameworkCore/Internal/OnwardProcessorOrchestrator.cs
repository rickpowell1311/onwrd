using Microsoft.Extensions.DependencyInjection;

namespace Onwrd.EntityFrameworkCore.Internal
{
    internal class OnwardProcessorOrchestrator : IOnwardProcessorOrchestrator
    {
        public async Task Process(
            (Event Event, object Contents) eventPair, 
            IServiceScope scope,
            CancellationToken cancellationToken)
        {
            var config = scope.ServiceProvider.GetService<OnwrdConfiguration>();

            if (config.UseOnwrdProcessor)
            {
                var onwrdProcessor = scope.ServiceProvider.GetService<IOnwardProcessor>();
                await onwrdProcessor.ProcessEvent(eventPair, cancellationToken);
            }

            if (config.UseOnwrdProcessors)
            {
                var onwrdProcessorType = typeof(IOnwardProcessor<>)
                    .MakeGenericType(eventPair.Contents.GetType());

                var onwrdProcessors = scope.ServiceProvider.GetServices(onwrdProcessorType);

                foreach (var onwrdProcessor in onwrdProcessors)
                {
                    await onwrdProcessor.ProcessEvent(onwrdProcessorType, eventPair, cancellationToken);
                }
            }
        }
    }
}
