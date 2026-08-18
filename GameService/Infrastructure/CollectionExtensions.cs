namespace GameService.Infrastructure;

internal static class CollectionExtensions
{
    public static bool AddIfMissing<T>(this ICollection<T> collection, T item)
    {
        if (collection.Contains(item)) {
            return false;
        }
        collection.Add(item);
        return true;
    }
}
