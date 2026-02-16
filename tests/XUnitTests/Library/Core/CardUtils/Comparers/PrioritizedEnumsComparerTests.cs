using System.Diagnostics;

using CoolCardGames.Library.Core.CardUtils.Comparers;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils.Comparers;

public class PrioritizedEnumsComparerTests
{
    private readonly PrioritizedEnumsComparer<Rank> _sut = new([Rank.Ace, Rank.King, Rank.Queen]);

    [Theory]
    [InlineData(Rank.King, Rank.Ace, Result.Before)]
    [InlineData(Rank.King, Rank.King, Result.Equal)]
    [InlineData(Rank.King, Rank.Queen, Result.After)]
    [InlineData(Rank.King, Rank.Two, Result.After)]
    [InlineData(Rank.Ace, Rank.King, Result.After)]
    [InlineData(Rank.King, Rank.King, Result.Equal)]
    [InlineData(Rank.Queen, Rank.King, Result.Before)]
    [InlineData(Rank.Two, Rank.King, Result.Before)]
    public void Test(Rank x, Rank y, Result expected)
    {
        var actual = _sut.Compare(x, y);
        switch (expected)
        {
            case Result.Equal:
                Assert.Equal(0, actual);
                break;
            case Result.After:
                Assert.True(actual < 0, $"{actual} was expected to be less than 0");
                break;
            case Result.Before:
                Assert.True(actual > 0, $"{actual} was expected to be greater than 0");
                break;
            default:
                throw new UnreachableException();
        }
    }

    public enum Result
    {
        Equal,
        After,
        Before
    }
}