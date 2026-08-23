using System.Text.Json.Serialization;

namespace GameService.Games.Pinochle;

/// <summary>Specifies the supported pinochle meld values.</summary>
[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PinochleMeld
{
    /// <summary>Represents the run option.</summary>
    Run,
    /// <summary>Represents the double run option.</summary>
    DoubleRun,
    /// <summary>Represents the royal marriage option.</summary>
    RoyalMarriage,
    /// <summary>Represents the double royal marriage option.</summary>
    DoubleRoyalMarriage,
    /// <summary>Represents the marriage option.</summary>
    Marriage,
    /// <summary>Represents the double marriage option.</summary>
    DoubleMarriage,
    /// <summary>Represents the pinochle option.</summary>
    Pinochle,
    /// <summary>Represents the double pinochle option.</summary>
    DoublePinochle,
    /// <summary>Represents the aces around option.</summary>
    AcesAround,
    /// <summary>Represents the double aces around option.</summary>
    DoubleAcesAround,
    /// <summary>Represents the kings around option.</summary>
    KingsAround,
    /// <summary>Represents the double kings around option.</summary>
    DoubleKingsAround,
    /// <summary>Represents the queens around option.</summary>
    QueensAround,
    /// <summary>Represents the double queens around option.</summary>
    DoubleQueensAround,
    /// <summary>Represents the jacks around option.</summary>
    JacksAround,
    /// <summary>Represents the double jacks around option.</summary>
    DoubleJacksAround,
    /// <summary>Represents the dix option.</summary>
    Dix
}
