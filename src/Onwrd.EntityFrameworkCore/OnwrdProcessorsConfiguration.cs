using System.Reflection;

namespace Onwrd.EntityFrameworkCore
{
    public class OnwrdProcessorsConfiguration
    {
        internal OnwrdProcessorTypeLibrary Library { get; private set; }

        internal OnwrdProcessorsConfiguration() 
        {
            Library = new OnwrdProcessorTypeLibrary();
        }

        public void ScanAssemblies(params Assembly[] assemblies)
        {
            var onwardProcessorTargets = assemblies
                .SelectMany(x => x.GetTypes()
                    .Where(y => y.GetInterfaces()
                        .Where(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IOnwardProcessor<>))
                        .Any()))
                .ToList();

            foreach (var onwrdProcessorTarget in onwardProcessorTargets)
            {
                var onwrdProcessorInterfaces = onwrdProcessorTarget.GetInterfaces()
                    .Where(z => z.IsGenericType && z.GetGenericTypeDefinition() == typeof(IOnwardProcessor<>))
                    .ToList();

                foreach (var onwrdProcessorInterface in onwrdProcessorInterfaces)
                {
                    var eventType = onwrdProcessorInterface.GetGenericArguments()[0];

                    Library.Add(eventType, onwrdProcessorTarget);
                }
            }
        }

        public void Register<TEvent, TOnwardProcessor>()
            where TOnwardProcessor : IOnwardProcessor<TEvent>
        {
            Library.Add(typeof(TEvent), typeof(TOnwardProcessor));
        }

        internal class OnwrdProcessorTypeLibrary
        {
            private readonly Dictionary<Type, HashSet<Type>> _dictionary;

            public IReadOnlyDictionary<Type, IEnumerable<Type>> Entries => _dictionary
                .ToDictionary(
                    x => x.Key,
                    x => x.Value.Select(y => y));

            public OnwrdProcessorTypeLibrary()
            {
                _dictionary = new Dictionary<Type, HashSet<Type>>();
            }

            public void Add(Type eventType, Type onwardProcessorType)
            {
                if (_dictionary.ContainsKey(eventType))
                {
                    _dictionary[eventType].Add(onwardProcessorType);
                    return;
                }

                _dictionary[eventType] = new HashSet<Type> { onwardProcessorType };
            }

            public IEnumerable<Type> this[Type eventType]
            {
                get => _dictionary[eventType];
            }
        }
    }
}
