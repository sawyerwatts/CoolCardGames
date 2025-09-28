using System.Diagnostics;

namespace CoolCardGames.Library.Core;

public record struct CircularCounter
{
    public int N { get; private set; }
    private readonly int _maxExclusive;

    public CircularCounter() => throw new NotSupportedException();

    public CircularCounter(int maxExclusive, bool startAtEnd = false)
    {
        if (maxExclusive < 0)
            throw new ArgumentException(
                $"A non-negative {nameof(maxExclusive)} is required but given {maxExclusive}");
        _maxExclusive = maxExclusive;

        if (startAtEnd)
            N = _maxExclusive - 1;
    }

    public CircularCounter(int seed, int maxExclusive)
    {
        if (maxExclusive < 0)
            throw new ArgumentException(
                $"A non-negative {nameof(maxExclusive)} is required but given {maxExclusive}");
        _maxExclusive = maxExclusive;

        if (seed < 0 || seed >= maxExclusive)
            throw new ArgumentException(
                $"{nameof(seed)} must be non-negative and less than {nameof(maxExclusive)}, but given {nameof(seed)} of {seed} and {nameof(maxExclusive)} of {maxExclusive}");
        N = seed;
    }

    public int CycleClockwise(int times = 1, bool updateInstance = true)
    {
        if (times < 1)
            throw new ArgumentException($"{nameof(times)} must be positive but given {times}");
        return Tick(times, updateInstance: updateInstance);
    }

    public int CycleCounterClockwise(int times = 1, bool updateInstance = true)
    {
        if (times < 1)
            throw new ArgumentException($"{nameof(times)} must be positive but given {times}");
        return Tick(times * -1, updateInstance: updateInstance);
    }

    // TODO: how much faster than modulus is this mess?
    public int Tick(int delta = 1, bool updateInstance = true)
    {
        if (delta == 0)
            return N;

        int n = N;
        int move = delta switch
        {
            < 0 => -1,
            > 0 => 1,
            0 => throw new UnreachableException(),
        };

        int absDelta = Math.Abs(delta);
        for (int i = 0; i < absDelta; i++)
        {
            n += move;
            if (n == _maxExclusive)
                n = 0;
            else if (n == -1)
                n = _maxExclusive - 1;
        }

        if (updateInstance)
            N = n;
        return n;
    }
}