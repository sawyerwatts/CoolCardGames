namespace CoolCardGames.Library.Core;

public interface IGame
{
    Task Play(CancellationToken cancellationToken);
}