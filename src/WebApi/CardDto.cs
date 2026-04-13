using CoolCardGames.Library.Core.CardTypes;

namespace CoolCardGames.WebApi;

public class CardDto
{
    public string Title { get; set; } = "";
}

public static class CardsToCardDtos
{
    public static IEnumerable<CardDto> ToDtos(this Cards cards)
    {
        return cards.Select(card => new CardDto { Title = card.ToString() });
    }
}