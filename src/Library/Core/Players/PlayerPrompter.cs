namespace CoolCardGames.Library.Core.Players;

// TODO: write unit tests for these funcs
// TODO: update these funcs to pass additional, human-readable validation info
/// <summary>
/// This class is an anti-corruption layer between the card games and the
/// <see cref="IPlayerSession{TCard}"/> so that:
/// <br /> - The card games' logic can be blissfully unaware of the multithreading (if implemented
/// that way).
/// <br /> - Reusably handle user input validation.
/// <br /> - The user can be hot swapped (see <see cref="IPlayerSession{TCard}"/> for more).
/// </summary>
public class PlayerPrompter<TCard, TPlayerState, TGameState>(
    IPlayerSession<TCard> playerSession,
    TGameState gameState,
    int gameStatePlayerIndex)
    where TCard : Card
    where TPlayerState : PlayerState<TCard>
    where TGameState : GameState<TCard, TPlayerState>
{
    public AccountCard AccountCard => playerSession.AccountCard;
    public int GameStatePlayerIndex => gameStatePlayerIndex;

    private TPlayerState PlayerState => gameState.Players[gameStatePlayerIndex];
    private Cards<TCard> Hand => PlayerState.Hand;

    /// <remarks>
    /// This will take the selected card out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// </remarks>
    /// <param name="validateChosenCard">
    /// This will take the current player hand and the pre-validated in-range index of the card to
    /// play, and return true iff it is valid to play that card.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<TCard> PlayCard(Func<Cards<TCard>, int, bool> validateChosenCard, CancellationToken cancellationToken)
    {
        bool validCardToPlay = false;
        int iCardToPlay = -1;
        while (!validCardToPlay)
        {
            iCardToPlay = await playerSession.PromptForIndexOfCardToPlay(Hand, cancellationToken);
            if (iCardToPlay < 0 || iCardToPlay >= Hand.Count)
                continue;

            validCardToPlay = validateChosenCard(Hand, iCardToPlay);
        }

        TCard cardToPlay = Hand[iCardToPlay];
        Hand.RemoveAt(iCardToPlay);
        return cardToPlay;
    }

    /// <remarks>
    /// This will take the selected card(s) out of the appropriate <see cref="GameState{TCard,TPlayerState}.Players"/>' <see cref="PlayerState{TCard}.Hand"/>.
    /// </remarks>
    /// <param name="validateChosenCards">
    /// This will take the current player hand and the pre-validated in-range and unique indexes of
    /// the cards to play, and return true iff it is valid to play those cards.
    /// </param>
    /// <param name="cancellationToken"></param>
    public async Task<Cards<TCard>> PlayCards(Func<Cards<TCard>, List<int>, bool> validateChosenCards, CancellationToken cancellationToken)
    {
        bool validCardsToPlay = false;
        List<int> iCardsToPlay = [];
        while (!validCardsToPlay)
        {
            iCardsToPlay = await playerSession.PromptForIndexesOfCardsToPlay(Hand, cancellationToken);
            if (iCardsToPlay.Count != iCardsToPlay.Distinct().Count())
                continue;

            if (iCardsToPlay.Any(iCardToPlay => iCardToPlay < 0 || iCardToPlay >= Hand.Count))
                continue;

            validCardsToPlay = validateChosenCards(Hand, iCardsToPlay);
        }

        Cards<TCard> cardsToPlay = new(capacity: iCardsToPlay.Count);
        foreach (int iCardToPlay in iCardsToPlay.OrderDescending())
        {
            cardsToPlay.Add(Hand[iCardToPlay]);
            Hand.RemoveAt(iCardToPlay);
        }

        return cardsToPlay;
    }
}