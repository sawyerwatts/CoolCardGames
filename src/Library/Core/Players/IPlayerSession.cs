using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

// TODO: update these methods to take whole game state?
// TODO: update these funcs to pass additional, human-readable validation info

/// <remarks>
/// <see cref="IPlayerSession{TCard}"/> and <see cref="PlayerPrompter{TCard,TPlayerState,TGameState}"/> being
/// two different types supports the following use case: if playing online, if someone goes offline,
/// the <see cref="PlayerPrompter{TCard,TPlayerState,TGameState}"/>'s session can be hot swapped to an AI
/// implementation without a game's logic needing to be aware of the change.
/// </remarks>
/// <typeparam name="TCard"></typeparam>
public interface IPlayerSession<TCard>
    where TCard : Card
{
    AccountCard AccountCard { get; }

    /// <remarks>
    /// This is nullable because if the player is not currently involved in a game, then they won't
    /// be receiving game events.
    /// </remarks>
    ChannelReader<GameEvent>? CurrentGamesEvents { get; set; }

    /// <summary>
    /// This will ask the player for any card to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    Task<int> PromptForIndexOfCardToPlay(Cards<TCard> cards, CancellationToken cancellationToken);

    /// <summary>
    /// This will ask the player for card(s) to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    Task<List<int>> PromptForIndexesOfCardsToPlay(Cards<TCard> cards, CancellationToken cancellationToken);
}