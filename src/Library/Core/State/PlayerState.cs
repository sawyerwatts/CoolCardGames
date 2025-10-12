namespace CoolCardGames.Library.Core.State;

public class PlayerState<TCard>
    where TCard : Card
{
    public Cards<TCard> Hand { get; set; } = [];
}