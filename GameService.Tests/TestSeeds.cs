namespace GameService.Tests;

internal static class TestSeeds
{
    public static Guid FromInt32(int value)
        => new(value, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0);
}
