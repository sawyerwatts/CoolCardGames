namespace CoolCardGames.Library.Core.Players;

public record PlayerAccountCard(string Id, string DisplayName)
{
    public override string ToString() => DisplayName;
}