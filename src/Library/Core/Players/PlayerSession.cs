using System.Collections.Concurrent;

namespace CoolCardGames.Library.Core.Players;

// TODO: update these funcs to pass additional, human-readable validation info

/// <remarks>
/// <see cref="PlayerSession{TCard}"/> and <see cref="Player{TCard,TPlayerState,TGameState}"/> being
/// two different types supports the following use case: if playing online, if someone goes offline,
/// the <see cref="Player{TCard,TPlayerState,TGameState}"/>'s session can be hot swapped to an AI
/// implementation without a game's logic needing to be aware of the change.
/// </remarks>
/// <typeparam name="TCard"></typeparam>
public abstract class PlayerSession<TCard>(AccountCard accountCard)
    where TCard : Card
{
    public AccountCard AccountCard => accountCard;
    public ConcurrentQueue<Core.GameEvents.GameEvent> UnprocessedGameEvents { get; } = new();

    // TODO: update these methods to take whole game state?
    public abstract Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken);

    public abstract Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken);
}