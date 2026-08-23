namespace GameService.Players;

/// <summary>Provides standard table team identifiers.</summary>
public static class TableTeams
{
    /// <summary>Identifies the north south.</summary>
    public const int NorthSouth = 1;
    /// <summary>Identifies the east west.</summary>
    public const int EastWest = 2;

    /// <summary>Gets the all.</summary>
    public static IReadOnlyList<int> All { get; } = [NorthSouth, EastWest];
}
