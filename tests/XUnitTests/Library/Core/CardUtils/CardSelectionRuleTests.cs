using CoolCardGames.Library.Core.CardUtils;

namespace CoolCardGames.XUnitTests.Library.Core.CardUtils;

public class CardSelectionRuleTests
{
    private readonly Cards _hand =
    [
        new(AceOfHearts.Instance),
        new(TwoOfHearts.Instance),
        new(FiveOfHearts.Instance),
        new(AceOfClubs.Instance),
    ];

    [Fact]
    public void TestCardSelectionRuleEnsureDefaultConstructorIsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => new CardSelectionRule());
    }

    [Fact]
    public void TestCardComboSelectionRuleEnsureDefaultConstructorIsNotSupported()
    {
        Assert.Throws<NotSupportedException>(() => new CardComboSelectionRule());
    }

    [Fact]
    public void TestCardSelectionRuleGivenValidIndexThenCallInjectedValidationFunc()
    {
        const int iExpected = 3;

        var rule = new CardSelectionRule(
            description: nameof(TestCardSelectionRuleGivenValidIndexThenCallInjectedValidationFunc),
            validateCard: (_, i) =>
            {
                Assert.Equal(iExpected, i);
                return true;
            });

        var validCardToPlay = rule.ValidateCard(_hand, 3);
        Assert.True(validCardToPlay);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void TestCardSelectionRuleGivenIndexOutOfRangeThenException(int iCardToPlay)
    {
        var rule = new CardSelectionRule(
            description: nameof(TestCardSelectionRuleGivenIndexOutOfRangeThenException),
            validateCard: (_, i) => true);

        Assert.Throws<ArgumentOutOfRangeException>(() => rule.ValidateCard(_hand, iCardToPlay));
    }

    [Fact]
    public void TestCardComboSelectionRuleGivenValidIndexesThenCallInjectedValidationFunc()
    {
        var rule = new CardComboSelectionRule(
            description: nameof(TestCardComboSelectionRuleGivenValidIndexesThenCallInjectedValidationFunc),
            validateCards: (_, indexes) =>
            {
                Assert.Equal(2, indexes.Count);
                Assert.Equal(3, indexes[0]);
                Assert.Equal(1, indexes[1]);
                return true;
            });

        var validCardsToPlay = rule.ValidateCards(_hand, [3, 1]);
        Assert.True(validCardsToPlay);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(4)]
    public void TestCardComboSelectionRuleGivenIndexOutOfRangeThenException(int iCardToPlay)
    {
        var rule = new CardComboSelectionRule(
            description: nameof(TestCardComboSelectionRuleGivenIndexOutOfRangeThenException),
            validateCards: (_, _) => true);

        Assert.Throws<ArgumentOutOfRangeException>(() => rule.ValidateCards(_hand, [0, iCardToPlay]));
    }

    [Fact]
    public void TestCardComboSelectionRuleGivenDuplicateIndexesThenException()
    {
        var rule = new CardComboSelectionRule(
            description: nameof(TestCardComboSelectionRuleGivenDuplicateIndexesThenException),
            validateCards: (_, _) => true);

        Assert.Throws<ArgumentException>(() => rule.ValidateCards(_hand, [0, 0]));
    }
}