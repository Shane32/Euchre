namespace GameService.Players;

/// <summary>Provides standard table seat identifiers.</summary>
public static class TableSeats
{
    /// <summary>Identifies the north.</summary>
    public const int North = 1;
    /// <summary>Identifies the east.</summary>
    public const int East = 2;
    /// <summary>Identifies the south.</summary>
    public const int South = 3;
    /// <summary>Identifies the west.</summary>
    public const int West = 4;

    /// <summary>Gets the all.</summary>
    public static IReadOnlyList<int> All { get; } = [North, East, South, West];
}
