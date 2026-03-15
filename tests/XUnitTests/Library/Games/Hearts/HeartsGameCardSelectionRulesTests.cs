using CoolCardGames.Library.Games.Hearts;

namespace CoolCardGames.XUnitTests.Library.Games.Hearts;

public class HeartsGameCardSelectionRulesTests
{
    private readonly Cards<HeartsCard> _hand =
    [
        new(TwoOfClubs.Instance),    // 0
        new(FiveOfClubs.Instance),   // 1

        new(AceOfDiamonds.Instance), // 2

        new(TwoOfHearts.Instance),   // 3
        new(FiveOfHearts.Instance),  // 4
        new(AceOfHearts.Instance),   // 5

        new(QueenOfSpades.Instance), // 6
    ];

    [Fact]
    public void TestFirstTrickOpeningCardMustBeTwoOfClubsGivenTwoOfClubsThenTrue()
    {
        var validCardToPlay = HeartsGameCardSelectionRules.FirstTrickOpeningCardMustBeTwoOfClubs.ValidateCard(_hand, 0);
        Assert.True(validCardToPlay);
    }

    [Fact]
    public void TestFirstTrickOpeningCardMustBeTwoOfClubsGivenOtherCardThenFalse()
    {
        var validCardToPlay = HeartsGameCardSelectionRules.FirstTrickOpeningCardMustBeTwoOfClubs.ValidateCard(_hand, 1);
        Assert.False(validCardToPlay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    public void TestFirstTrickCannotHavePointsGivenNoPointsThenTrue(int iCardToPlay)
    {
        var validCardToPlay = HeartsGameCardSelectionRules.FirstTrickCannotHavePoints.ValidateCard(_hand, iCardToPlay);
        Assert.True(validCardToPlay);
    }

    [Theory]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void TestFirstTrickCannotHavePointsGivenPointsThenFalse(int iCardToPlay)
    {
        var validCardToPlay = HeartsGameCardSelectionRules.FirstTrickCannotHavePoints.ValidateCard(_hand, iCardToPlay);
        Assert.False(validCardToPlay);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(6)]
    public void TestHeartsCanOnlyBeLeadOnceBrokenGivenHeartsIsBrokenThenAnySuitCanBePlayed(int iCardToPlay)
    {
        var rule = HeartsGameCardSelectionRules.HeartsCanOnlyBeLeadOnceBroken(isHeartsBroken: true);
        var validCardToPlay = rule.ValidateCard(_hand, iCardToPlay);
        Assert.True(validCardToPlay);
    }

    [Theory]
    [InlineData(1, true)]
    [InlineData(2, true)]
    [InlineData(3, false)]
    [InlineData(6, true)]
    public void TestHeartsCanOnlyBeLeadOnceBrokenGivenHeartsIsNotBrokenThenHeartsCannotBePlayed(int iCardToPlay, bool expected)
    {
        var rule = HeartsGameCardSelectionRules.HeartsCanOnlyBeLeadOnceBroken(isHeartsBroken: false);
        var validCardToPlay = rule.ValidateCard(_hand, iCardToPlay);
        Assert.Equal(expected, validCardToPlay);
    }
}