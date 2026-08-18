namespace GameService.Players;

public static class TableSeats
{
    public const int North = 1;
    public const int East = 2;
    public const int South = 3;
    public const int West = 4;

    public static IReadOnlyList<int> All { get; } = [North, East, South, West];
}
