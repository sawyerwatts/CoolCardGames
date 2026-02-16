using System.Diagnostics;

using CoolCardGames.Library.Core.CardUtils.Comparers;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils.Comparers;

public class PrioritizedEnumsComparerTests
{
    private readonly PrioritizedEnumsComparer<Rank> _sut = new([Rank.Ace, Rank.King, Rank.Queen]);

    [Theory]
    [InlineData(Rank.King, Result.Lower, Rank.Ace)]
    [InlineData(Rank.King, Result.Equal, Rank.King)]
    [InlineData(Rank.King, Result.Higher, Rank.Queen)]
    [InlineData(Rank.King, Result.Higher, Rank.Two)]
    [InlineData(Rank.Ace, Result.Higher, Rank.King)]
    [InlineData(Rank.King, Result.Equal, Rank.King)]
    [InlineData(Rank.Queen, Result.Lower, Rank.King)]
    [InlineData(Rank.Two, Result.Lower, Rank.King)]
    public void Test(Rank x, Result expected, Rank y)
    {
        var actual = _sut.Compare(x, y);
        switch (expected)
        {
            case Result.Equal:
                Assert.Equal(0, actual);
                break;
            case Result.Higher:
                Assert.True(actual < 0, $"{actual} was expected to be less than 0");
                break;
            case Result.Lower:
                Assert.True(actual > 0, $"{actual} was expected to be greater than 0");
                break;
            default:
                throw new UnreachableException();
        }
    }

    public enum Result
    {
        Equal,
        Higher,
        Lower,
    }
}