using System.Text.Json.Serialization;

namespace GameService.Games.Pinochle;

[JsonConverter(typeof(CamelCaseJsonStringEnumConverter))]
public enum PinochleMeld
{
    Run,
    DoubleRun,
    RoyalMarriage,
    DoubleRoyalMarriage,
    Marriage,
    DoubleMarriage,
    Pinochle,
    DoublePinochle,
    AcesAround,
    DoubleAcesAround,
    KingsAround,
    DoubleKingsAround,
    QueensAround,
    DoubleQueensAround,
    JacksAround,
    DoubleJacksAround,
    Dix
}
