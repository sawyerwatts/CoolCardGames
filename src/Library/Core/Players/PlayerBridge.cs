using System.Threading.Channels;

namespace CoolCardGames.Library.Core.Players;

/// <summary>
/// This class exists so that if a player disconnects, they can be seamlessly swapped to an AI.
/// </summary>
public class PlayerBridge(IPlayer player /*, AiPlayerFactory aiFactory */)
    : IPlayer
{
    public PlayerAccountCard AccountCard => player.AccountCard;

    public Disposable JoinGame(ChannelReader<GameEventEnvelope> currGamesEvents, IGameEventPublisher currGameEventPublisher)
    {
        return player.JoinGame(currGamesEvents, currGameEventPublisher);
    }

    public Task<Card> PromptForValidCardAndPlay(Cards cards, CardSelectionRule cardSelectionRule, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardAndPlay(cards, cardSelectionRule, cancellationToken, reveal);
    }

    public Task<Card> PromptForValidCardAndPlay(Cards cards, List<CardSelectionRule> cardSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardAndPlay(cards, cardSelectionRules, cancellationToken, reveal);
    }

    public Task<Cards> PromptForValidCardsAndPlay(Cards cards, CardComboSelectionRule cardComboSelectionRule, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardsAndPlay(cards, cardComboSelectionRule, cancellationToken, reveal);
    }

    public Task<Cards> PromptForValidCardsAndPlay(Cards cards, List<CardComboSelectionRule> cardComboSelectionRules, CancellationToken cancellationToken, bool reveal = true)
    {
        return player.PromptForValidCardsAndPlay(cards, cardComboSelectionRules, cancellationToken, reveal);
    }
}