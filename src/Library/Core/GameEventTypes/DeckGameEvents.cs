namespace CoolCardGames.Library.Core.GameEventTypes;

public abstract partial record GameEvent
{
    public record DeckShuffled() : GameEvent("The deck was shuffled")
    {
        public static readonly DeckShuffled Singleton = new();
    }

    public record DeckCut() : GameEvent("The deck was cut")
    {
        public static readonly DeckCut Singleton = new();
    }

    public record DeckDealt(int NumHands) : GameEvent($"The deck was dealt to {NumHands} hands");
}