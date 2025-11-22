using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

// TODO: update these methods to take whole game state?
// TODO: update these funcs to pass additional, human-readable validation info

public interface IPlayer<TCard>
    where TCard : Card
{
    PlayerAccountCard PlayerAccountCard { get; }

    /// <remarks>
    /// This is nullable because if the player has not yet been in a game, then this will not have
    /// been initialized. Do note that once a game completes, this property may contain a non-null,
    /// but completed, channel reader.
    /// </remarks>
    ChannelReader<GameEventEnvelope>? CurrentGamesEvents { get; set; }

    /// <summary>
    /// This will ask the player for any card to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    Task<int> PromptForIndexOfCardToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken);

    /// <summary>
    /// This will ask the player for card(s) to play. Validation and removal from hand will be handled elsewhere.
    /// </summary>
    Task<List<int>> PromptForIndexesOfCardsToPlay(uint prePromptEventId, Cards<TCard> cards, CancellationToken cancellationToken);
}