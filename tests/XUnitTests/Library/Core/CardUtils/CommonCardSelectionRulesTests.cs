using CoolCardGames.Library.Core.CardUtils;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils;

public class CommonCardSelectionRulesTests
{
    private readonly Cards _hand =
    [
        new(AceOfHearts.Instance),
        new(TwoOfHearts.Instance),
        new(FiveOfHearts.Instance),
        new(AceOfClubs.Instance),
    ];

    [Fact]
    public void TestIsSuitFollowedIfPossibleGivenHandHasSuitAndCardIsSuitThenTrue()
    {
        var rule = CommonCardSelectionRules.IsSuitFollowedIfPossible(Suit.Hearts);
        var validCardToPlay = rule.ValidateCard(_hand, 1);
        Assert.True(validCardToPlay);
    }

    [Fact]
    public void TestIsSuitFollowedIfPossibleGivenHandHasSuitAndCardIsNotSuitThenFalse()
    {
        var rule = CommonCardSelectionRules.IsSuitFollowedIfPossible(Suit.Hearts);
        var validCardToPlay = rule.ValidateCard(_hand, 3);
        Assert.False(validCardToPlay);
    }

    [Fact]
    public void TestIsSuitFollowedIfPossibleGivenHandLacksSuitAndCardIsNotSuitThenTrue()
    {
        var rule = CommonCardSelectionRules.IsSuitFollowedIfPossible(Suit.Spades);
        var validCardToPlay = rule.ValidateCard(_hand, 3);
        Assert.True(validCardToPlay);
    }

    [Fact]
    public void TestLimitNumberOfCardsSelectedExpectingSpecificNumberOfCardsAndGivenThatManyCardsThenTrue()
    {
        var rule = CommonCardSelectionRules.LimitNumberOfCardsSelected(2);
        var validCardsToPlay = rule.ValidateCards(_hand, [0, 1]);
        Assert.True(validCardsToPlay);
    }

    [Fact]
    public void TestLimitNumberOfCardsSelectedExpectingSpecificNumberOfCardsAndGivenTooFewCardsThenFalse()
    {
        var rule = CommonCardSelectionRules.LimitNumberOfCardsSelected(2);
        var validCardsToPlay = rule.ValidateCards(_hand, [0]);
        Assert.False(validCardsToPlay);
    }

    [Fact]
    public void TestLimitNumberOfCardsSelectedExpectingSpecificNumberOfCardsAndGivenTooManyCardsThenFalse()
    {
        var rule = CommonCardSelectionRules.LimitNumberOfCardsSelected(2);
        var validCardsToPlay = rule.ValidateCards(_hand, [0, 1, 2]);
        Assert.False(validCardsToPlay);
    }

    [Theory]
    [MemberData(nameof(DataForTestLimitNumberOfCardsSelectedExpectingRangeAndGivenValidNumberOfCardsThenTrue))]
    public void TestLimitNumberOfCardsSelectedExpectingRangeAndGivenValidNumberOfCardsThenTrue(List<int> iCardsToPlay)
    {
        var rule = CommonCardSelectionRules.LimitNumberOfCardsSelected(minCardsToPlay: 1, maxCardsToPlay: 3);
        var validCardsToPlay = rule.ValidateCards(_hand, iCardsToPlay);
        Assert.True(validCardsToPlay);
    }

    public static IEnumerable<object[]> DataForTestLimitNumberOfCardsSelectedExpectingRangeAndGivenValidNumberOfCardsThenTrue =>
        new List<object[]>
        {
            new object[] { new List<int>() { 0 } },
            new object[] { new List<int>() { 0, 1 } },
            new object[] { new List<int>() { 0, 1, 2 } },
        };

    [Theory]
    [MemberData(nameof(DataForTestLimitNumberOfCardsSelectedExpectingRangeAndGivenInvalidNumberOfCardsThenFalse))]
    public void TestLimitNumberOfCardsSelectedExpectingRangeAndGivenInvalidNumberOfCardsThenFalse(List<int> iCardsToPlay)
    {
        var rule = CommonCardSelectionRules.LimitNumberOfCardsSelected(minCardsToPlay: 1, maxCardsToPlay: 3);
        var validCardsToPlay = rule.ValidateCards(_hand, iCardsToPlay);
        Assert.False(validCardsToPlay);
    }

    public static IEnumerable<object[]> DataForTestLimitNumberOfCardsSelectedExpectingRangeAndGivenInvalidNumberOfCardsThenFalse =>
        new List<object[]>
        {
            new object[] { new List<int>() { } },
            new object[] { new List<int>() { 0, 1, 2, 3 } },
        };

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, 0)]
    public void TestLimitNumberOfCardsSelectedGivenBadBounds(int minCardsToPlay, int maxCardsToPlay)
    {
        Assert.Throws<ArgumentException>(() =>
            CommonCardSelectionRules.LimitNumberOfCardsSelected(minCardsToPlay: minCardsToPlay, maxCardsToPlay: maxCardsToPlay));
    }
}