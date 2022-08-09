using System.Reflection;
using System.Text.Json;

namespace Onwrd.EntityFrameworkCore
{
    internal class Event
    {
        public Guid Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime? DispatchedOn { get; set; }

        public string TypeId { get; set; }

        public string Contents { get; set; }

        public string AssemblyName { get; set; }

        internal static Event FromContents(object contents)
        {
            return new Event
            {
                TypeId = contents.GetType().FullName,
                AssemblyName = contents.GetType().Assembly.GetName().Name,
                Contents = JsonSerializer.Serialize(contents),
                CreatedOn = DateTime.UtcNow,
                DispatchedOn = null,
                Id = Guid.NewGuid()
            };
        }

        internal object DeserializeContents()
        {
            var targetType = AppDomain.CurrentDomain
                .GetAssemblies()
                .Select(a => a.GetType(TypeId, throwOnError: false))
                .FirstOrDefault(x => x != null && x.Assembly.GetName().Name == AssemblyName);

            if (targetType == null)
            {
                throw new TypeLoadException($"Unable to load type {TypeId} from assembly '{AssemblyName}'");
            }

            return JsonSerializer.Deserialize(Contents, targetType);
        }
    }
}