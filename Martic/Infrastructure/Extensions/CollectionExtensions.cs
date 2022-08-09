namespace Martic.Infrastructure.Extensions
{
    public static class CollectionExtensions
    {
        public static bool IsNullOrEmpty<T>(this IEnumerable<T> source)
        {
            if (source is null)
                return true;

            return !source.Any();
        }
    }
}
