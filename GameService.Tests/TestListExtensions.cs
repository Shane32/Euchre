namespace GameService.Tests;

internal static class TestListExtensions
{
    public static int FindIndex<T>(this IReadOnlyList<T> list, Func<T, bool> predicate)
    {
        for (var index = 0; index < list.Count; index++) {
            if (predicate(list[index])) {
                return index;
            }
        }
        return -1;
    }
}
