namespace Onwrd.EntityFrameworkCore.Internal
{
    internal static class ListExtensions
    {
        public static bool TryPop<T>(this List<T> collection, out T popped)
        {
            popped = collection.FirstOrDefault();

            if (popped != null && !popped.Equals(default(T)))
            {
                collection.RemoveAt(0);
            }

            return popped != null && !popped.Equals(default(T));
        }
    }
}
