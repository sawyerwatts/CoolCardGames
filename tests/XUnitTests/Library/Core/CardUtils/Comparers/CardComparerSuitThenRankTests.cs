using System.Collections;
using System.Diagnostics;

using CoolCardGames.Library.Core.CardUtils.Comparers;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils.Comparers;

public class CardComparerSuitThenRankTests
{
    [Theory]
    [ClassData(typeof(TestData))]
    public void Test(CardValue? x, Result expected, CardValue? y)
    {
        var sut = new CardComparerSuitThenRank<Card>(
            suitPriorities: [Suit.Spades],
            rankPriorities: [Rank.Ace, Rank.King, Rank.Queen]);

        var actual = sut.Compare(
            x: x is null ? null : new Card(x),
            y: y is null ? null : new Card(y));
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

    public class TestData : IEnumerable<Object[]>
    {
        public IEnumerator<object[]> GetEnumerator()
        {
            yield return [AceOfDiamonds.Instance, Result.Higher, KingOfDiamonds.Instance];
            yield return [AceOfDiamonds.Instance, Result.Lower, KingOfSpades.Instance];
            yield return [AceOfDiamonds.Instance, Result.Lower, TwoOfSpades.Instance];
            yield return [TwoOfDiamonds.Instance, Result.Lower, TwoOfSpades.Instance];
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }

    public enum Result
    {
        Equal,
        Higher,
        Lower,
    }
}