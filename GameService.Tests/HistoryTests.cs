using System.Buffers.Binary;
using System.Text.Json;

namespace GameService.Tests;

public sealed class HistoryTests
{
    [Fact]
    public void HistoryStartsWithTheInitialDealAndItsRandomSeed()
    {
        var game = new EuchreGameService("history", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(12345));

        Assert.Empty(game.GetHistory().Entries);
        game.StartGame();

        var history = game.GetHistory();
        var deal = Assert.Single(history.Entries);
        Assert.Equal(GameHistorySchemaVersions.Version1, history.SchemaVersion);
        Assert.Equal(TestSeeds.FromInt32(12345), history.BaseRandomSeed);
        Assert.Equal(1, deal.Sequence);
        Assert.Equal(1, deal.Revision);
        Assert.Equal(ActionTransport.System, deal.Transport);
        Assert.Equal(GameActionType.Deal, deal.Action);
        Assert.Equal([TestSeeds.FromInt32(12345)], deal.RandomSeeds);
    }

    [Fact]
    public void IdempotentRetriesAndRejectedActionsDoNotAddHistoryEntries()
    {
        var game = new EuchreGameService("history", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(100));
        game.StartGame();
        game.Pass("p2", "pass-1", expectedRevision: 1);
        game.Pass("p2", "pass-1", expectedRevision: 1);

        Assert.Throws<GameRuleException>(() => game.Pass("p2", "not-your-turn"));

        var history = game.GetHistory();
        Assert.Equal(2, history.Entries.Count);
        Assert.Equal(GameActionType.Pass, history.Entries[1].Action);
        Assert.Equal("p2", history.Entries[1].PlayerId);
        Assert.Equal(1, history.Entries[1].ExpectedRevision);
    }

    [Fact]
    public void EuchreStateAndHistoryRestoreAfterJsonRoundTrip()
    {
        var original = new EuchreGameService("history", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(8675309));
        original.StartGame();
        original.OrderUp("p2", alone: false, "order", expectedRevision: 1);
        original.Discard("p1", 0, "discard", expectedRevision: 2);
        original.Play("p2", 0, "play", expectedRevision: 3);

        var json = JsonSerializer.Serialize(original.GetHistory());
        var persisted = JsonSerializer.Deserialize<GameHistory>(json)!;
        var restored = Assert.IsType<EuchreGameService>(GameServiceFactory.Restore(persisted));

        AssertEquivalentPlayerStates(original, restored);
        Assert.Equal(json, JsonSerializer.Serialize(restored.GetHistory()));

        var retry = restored.Play("p2", 0, "play", expectedRevision: 3);
        Assert.Equal(4, retry.Revision);
        Assert.Equal(4, restored.GetHistory().Entries.Count);
    }

    [Fact]
    public void AutomaticRedealRecordsAndReplaysTheNewDealSeed()
    {
        var baseSeed = TestSeeds.FromInt32(400);
        var original = new EuchreGameService("redeal-history", TestHelpers.PLAYERS, seed: baseSeed);
        original.StartGame();
        PassAllBiddingRounds(original);

        var history = original.GetHistory();
        Assert.Equal([baseSeed], history.Entries[0].RandomSeeds);
        Assert.Equal([DeriveDealSeed(baseSeed, 1)], history.Entries[^1].RandomSeeds);
        Assert.Equal(GameActionType.Pass, history.Entries[^1].Action);

        var restored = EuchreGameService.Restore(history);
        AssertEquivalentPlayerStates(original, restored);
        Assert.Equal(2, restored.GetGameState("p1").DealNumber);
    }

    [Fact]
    public void PinochleHistoryRestoresBiddingMeldAndPlayState()
    {
        var options = new Dictionary<string, object?> {
            [PinochleOptionNames.RequireTrumpMarriage] = false
        };
        var original = new PinochleGameService("pinochle-history", TestHelpers.PLAYERS, seed: TestSeeds.FromInt32(2468), options);
        original.StartGame();
        original.Bid("p2", 20, "bid");
        original.Pass("p3", "pass-1");
        original.Pass("p4", "pass-2");
        original.Pass("p1", "pass-3");
        original.ChooseTrump("p2", CardSuit.Hearts, "trump");
        original.Play("p2", 0, "play");

        var restored = PinochleGameService.Restore(original.GetHistory());

        AssertEquivalentPlayerStates(original, restored);
        Assert.Equal(original.GetHistory().Entries.Count, restored.GetHistory().Entries.Count);
    }

    private static void PassAllBiddingRounds(EuchreGameService game)
    {
        game.Pass("p2", "first-1");
        game.Pass("p3", "first-2");
        game.Pass("p4", "first-3");
        game.Pass("p1", "first-4");
        game.Pass("p2", "second-1");
        game.Pass("p3", "second-2");
        game.Pass("p4", "second-3");
        game.Pass("p1", "second-4");
    }

    private static void AssertEquivalentPlayerStates(IGameService expected, IGameService actual)
    {
        foreach (var player in TestHelpers.PLAYERS) {
            Assert.Equal(
                JsonSerializer.Serialize(expected.GetGameState(player.PlayerId)),
                JsonSerializer.Serialize(actual.GetGameState(player.PlayerId)));
        }
    }

    private static Guid DeriveDealSeed(Guid baseSeed, int dealSequence)
    {
        var random = new ReplayableRandom(baseSeed);
        Span<byte> seedBytes = stackalloc byte[16];
        for (var currentDeal = 1; currentDeal <= dealSequence; currentDeal++) {
            for (var offset = 0; offset < seedBytes.Length; offset += sizeof(int)) {
                BinaryPrimitives.WriteInt32LittleEndian(
                    seedBytes[offset..],
                    random.Next(int.MinValue, int.MaxValue));
            }
        }
        return new Guid(seedBytes);
    }
}
