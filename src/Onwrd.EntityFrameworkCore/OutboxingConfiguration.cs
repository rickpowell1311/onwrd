using Microsoft.Extensions.DependencyInjection;
using Onwrd.EntityFrameworkCore.Internal;

namespace Onwrd.EntityFrameworkCore
{
    public class OutboxingConfiguration
    {
        private readonly IServiceCollection serviceCollection;

        internal OutboxingConfiguration(IServiceCollection serviceCollection)
        {
            this.serviceCollection = serviceCollection;
        }

        public void UseOnwardProcessor<T>()
            where T : class, IOnwardProcessor
        {
            if (!this.serviceCollection.Any(x => x.ServiceType == typeof(T)))
            {
                this.serviceCollection.AddTransient<IOnwardProcessor, T>();
            }
        }
    }
}
