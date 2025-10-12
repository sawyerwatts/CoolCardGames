namespace CoolCardGames.Library.Core;

public record AccountCard(string Id, string DisplayName)
{
    public override string ToString() => $"{DisplayName} ({Id})";
}