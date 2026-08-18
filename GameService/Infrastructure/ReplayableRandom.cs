using System.Buffers.Binary;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;

namespace GameService.Infrastructure;

/// <summary>
/// Produces a reproducible sequence of integers from HMAC-SHA-256 blocks.
/// Each block supplies eight 32-bit values before the block index advances.
/// </summary>
public sealed class ReplayableRandom
{
    private const int HashSize = 32;
    private const int ValuesPerBlock = HashSize / sizeof(uint);

    [InlineArray(HashSize)]
    private struct HashBlock
    {
        private byte _element0;
    }

    private HashBlock _block;
    private int _index;
    private int _blockPosition = ValuesPerBlock;

    public ReplayableRandom(Guid seed)
    {
        Seed = seed;
    }

    public Guid Seed { get; }

    private Span<byte> Block => _block;

    public int Next(int exclusiveMaximum)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(exclusiveMaximum);
        return Next(0, exclusiveMaximum);
    }

    public int Next(int inclusiveMinimum, int exclusiveMaximum)
    {
        if (inclusiveMinimum > exclusiveMaximum) {
            throw new ArgumentOutOfRangeException(nameof(inclusiveMinimum), "The minimum cannot exceed the maximum.");
        }
        var value = NextUInt32();
        if (inclusiveMinimum == exclusiveMaximum) {
            return inclusiveMinimum;
        }
        var range = (ulong)((long)exclusiveMaximum - inclusiveMinimum);
        var offset = (long)(((ulong)value * range) >> 32);
        return (int)(inclusiveMinimum + offset);
    }

    private static void GetRandomBlock(Guid seed, int index, Span<byte> destination)
    {
        Span<byte> key = stackalloc byte[16];
        if (!seed.TryWriteBytes(key)) {
            throw new InvalidOperationException("The random seed could not be written to the HMAC key buffer.");
        }

        Span<byte> data = stackalloc byte[4];
        BinaryPrimitives.WriteInt32LittleEndian(data, index);

        var bytesWritten = HMACSHA256.HashData(key, data, destination);
        if (bytesWritten != HashSize) {
            throw new CryptographicException($"HMAC-SHA-256 wrote {bytesWritten} bytes instead of {HashSize}.");
        }
    }

    private uint NextUInt32()
    {
        if (_blockPosition == ValuesPerBlock) {
            FillBlock();
            _blockPosition = 0;
        }

        var value = BinaryPrimitives.ReadUInt32LittleEndian(
            Block.Slice(_blockPosition * sizeof(uint), sizeof(uint)));
        _blockPosition++;
        if (_blockPosition == ValuesPerBlock) {
            _index++;
        }
        return value;
    }

    private void FillBlock() => GetRandomBlock(Seed, _index, Block);
}
