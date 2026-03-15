using System.Diagnostics;

using CoolCardGames.Library.Core.CardUtils.Comparers;
using CoolCardGames.Library.Core.Players;

namespace CoolCardGames.Library.Games.Hearts;

public interface IHeartsSetupRound
{
    Task Go(HeartsGameState gameState, PassDirection passDirection, CancellationToken cancellationToken);
}

public class HeartsSetupRound(IDealer dealer, IGameEventPublisher gameEventPublisher, List<IPlayer<HeartsCard>> players) : IHeartsSetupRound
{
    public static readonly IComparer<HeartsCard> HandSortingComparer = new CardComparerSuitThenRank<HeartsCard>(
        suitPriorities:
        [
            Suit.Spades,
            Suit.Hearts,
            Suit.Clubs,
            Suit.Diamonds,
        ],
        rankPriorities: CommonRankPriorities.AceHighAscending);

    public async Task Go(HeartsGameState gameState, PassDirection passDirection, CancellationToken cancellationToken)
    {
        await gameEventPublisher.Publish(GameEvent.SettingUpNewRound.Singleton, cancellationToken);

        ResetState(gameState);
        await InitHands(gameState, cancellationToken);

        if (passDirection is PassDirection.Hold)
        {
            await gameEventPublisher.Publish(HeartsGameEvent.HoldEmRound.Singleton, cancellationToken);
        }
        else
        {
            await HavePlayersPassCards(gameState, passDirection, cancellationToken);
        }

        await gameEventPublisher.Publish(GameEvent.BeginningNewRound.Singleton, cancellationToken);
    }

    public void ResetState(HeartsGameState gameState)
    {
        gameState.IsFirstTrick = true;
        gameState.IsHeartsBroken = false;
        foreach (var playerState in gameState.Players)
            playerState.TricksTaken.Clear();
    }

    public async Task InitHands(HeartsGameState gameState, CancellationToken cancellationToken)
    {
        // TODO: could preserve and reshuffle cards instead of reinstantiating every round
        var hands = await dealer.ShuffleCutDeal(
            deck: HeartsCard.MakeDeck(Decks.Standard52()),
            numHands: HeartsGame.NumPlayers,
            cancellationToken);

        for (var i = 0; i < HeartsGame.NumPlayers; i++)
        {
            var hand = hands[i];
            hand.CardComparer = HandSortingComparer;
            gameState.Players[i].Hand = hand;
            await gameEventPublisher.Publish(new GameEvent.HandGiven(players[i].AccountCard, hand.Count),
                cancellationToken);
        }
    }

    public async Task HavePlayersPassCards(HeartsGameState gameState, PassDirection passDirection, CancellationToken cancellationToken)
    {
        await gameEventPublisher.Publish(new HeartsGameEvent.GetReadyToPass(passDirection), cancellationToken);
        List<Task<Cards<HeartsCard>>> takeCardsFromPlayerTasks = new(capacity: HeartsGame.NumPlayers);
        var ruleSelectThreeCards = CommonRules.LimitNumberOfCardsSelected<HeartsCard>(exactNumberOfCardsToPlay: 3);
        for (var i = 0; i < HeartsGame.NumPlayers; i++)
        {
            var task = players[i].PromptForValidCardsAndPlay(
                cards: gameState.Players[i].Hand,
                cardComboSelectionRule: ruleSelectThreeCards,
                cancellationToken,
                reveal: false);
            takeCardsFromPlayerTasks.Add(task);
        }

        await Task.WhenAll(takeCardsFromPlayerTasks).WaitAsync(cancellationToken);

        for (var iSourcePlayer = 0; iSourcePlayer < HeartsGame.NumPlayers; iSourcePlayer++)
        {
            CircularCounter sourcePlayerPosition = new(iSourcePlayer, HeartsGame.NumPlayers);
            var iTargetPlayer = passDirection switch
            {
                PassDirection.Left => sourcePlayerPosition.CycleClockwise(updateInstance: false),
                PassDirection.Right => sourcePlayerPosition.CycleCounterClockwise(updateInstance: false),
                PassDirection.Across => sourcePlayerPosition.CycleClockwise(times: 2, updateInstance: false),
                _ => throw new UnreachableException(
                    $"Passing {passDirection} from {nameof(iSourcePlayer)} {iSourcePlayer}"),
            };

            var cardsToPass = takeCardsFromPlayerTasks[iSourcePlayer].Result;
            gameState.Players[iTargetPlayer].Hand.AddRange(cardsToPass);
            gameState.Players[iTargetPlayer].Hand = gameState.Players[iTargetPlayer].Hand;
            await gameEventPublisher.Publish(
                new GameEvent.PlayerReceivedHiddenCards(players[iTargetPlayer].AccountCard, cardsToPass.Count),
                cancellationToken);
        }

        await gameEventPublisher.Publish(new HeartsGameEvent.CardsPassed(passDirection), cancellationToken);
    }

}