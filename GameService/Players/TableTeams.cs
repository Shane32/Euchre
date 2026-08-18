namespace GameService.Players;

public static class TableTeams
{
    public const int NorthSouth = 1;
    public const int EastWest = 2;

    public static IReadOnlyList<int> All { get; } = [NorthSouth, EastWest];
}
