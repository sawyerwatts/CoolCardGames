namespace CoolCardGames.Library.Core.Players;

public record AccountCard(string Id, string DisplayName)
{
    public override string ToString() => $"{DisplayName} ({Id})";
}