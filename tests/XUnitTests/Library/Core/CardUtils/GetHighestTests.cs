using CoolCardGames.Library.Core.CardUtils;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils;

public class GetHighestTests
{
    [Fact]
    public void GetHighestOfHighValue()
    {
        List<Rank> rankPriorities =
        [
            Rank.Ace,
            Rank.Two,
            Rank.Three,
        ];

        Rank highestGivenRank = GetHighest.Of(rankPriorities, [Rank.Two, Rank.Four, Rank.Three, Rank.Ace]);

        Assert.Equal(Rank.Ace, highestGivenRank);
    }

    [Fact]
    public void GetHighestOfMidValue()
    {
        List<Rank> rankPriorities =
        [
            Rank.Ace,
            Rank.Two,
            Rank.Three,
        ];

        Rank highestGivenRank = GetHighest.Of(rankPriorities, [Rank.Four, Rank.Three, Rank.Two, Rank.Three]);

        Assert.Equal(Rank.Two, highestGivenRank);
    }

    [Fact]
    public void GetHighestOfLowValue()
    {
        List<Rank> rankPriorities =
        [
            Rank.Ace,
            Rank.Two,
            Rank.Three,
        ];

        Rank highestGivenRank = GetHighest.Of(rankPriorities, [Rank.Four, Rank.Three]);

        Assert.Equal(Rank.Three, highestGivenRank);
    }
}