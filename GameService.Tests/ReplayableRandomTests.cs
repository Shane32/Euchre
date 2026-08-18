using System.Buffers.Binary;
using System.Security.Cryptography;

namespace GameService.Tests;

public sealed class ReplayableRandomTests
{
    private static readonly Guid s_seed = Guid.Parse("00112233-4455-6677-8899-aabbccddeeff");

    [Fact]
    public void ExposesItsGuidSeed()
    {
        Assert.Equal(s_seed, new ReplayableRandom(s_seed).Seed);
    }

    [Fact]
    public void UsesAllEightWordsBeforeAdvancingToTheNextHashBlock()
    {
        var random = new ReplayableRandom(s_seed);

        var actual = Enumerable.Range(0, 9)
            .Select(_ => random.Next(-1_000_000, 1_000_000))
            .ToArray();
        var expected = Enumerable.Range(0, 9)
            .Select(call => ExpectedValue(s_seed, call / 8, call % 8, -1_000_000, 1_000_000))
            .ToArray();

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void SameSeedProducesTheSameSequence()
    {
        var first = new ReplayableRandom(s_seed);
        var second = new ReplayableRandom(s_seed);

        Assert.Equal(
            Enumerable.Range(0, 100).Select(_ => first.Next(1000)),
            Enumerable.Range(0, 100).Select(_ => second.Next(1000)));
    }

    [Theory]
    [InlineData(-20, -10)]
    [InlineData(-10, 10)]
    [InlineData(10, 20)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void TwoArgumentNextReturnsValuesWithinTheRequestedRange(int minimum, int maximum)
    {
        var random = new ReplayableRandom(s_seed);

        Assert.All(
            Enumerable.Range(0, 100).Select(_ => random.Next(minimum, maximum)),
            value => Assert.InRange(value, minimum, maximum - 1));
    }

    [Fact]
    public void EqualBoundsReturnTheBoundAndConsumeOneWord()
    {
        var withEqualCall = new ReplayableRandom(s_seed);
        var direct = new ReplayableRandom(s_seed);

        Assert.Equal(42, withEqualCall.Next(42, 42));
        direct.Next(100);
        Assert.Equal(direct.Next(100), withEqualCall.Next(100));
    }

    private static int ExpectedValue(Guid seed, int index, int word, int minimum, int maximum)
    {
        Span<byte> block = stackalloc byte[32];
        GetExpectedBlock(seed, index, block);
        var value = BinaryPrimitives.ReadUInt32LittleEndian(block[(word * sizeof(uint))..]);
        var range = (ulong)((long)maximum - minimum);
        var offset = (long)(((ulong)value * range) >> 32);
        return (int)(minimum + offset);
    }

    private static void GetExpectedBlock(Guid seed, int index, Span<byte> destination)
    {
        Span<byte> key = stackalloc byte[16];
        seed.TryWriteBytes(key);
        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, index);
        HMACSHA256.HashData(key, data, destination);
    }
}
