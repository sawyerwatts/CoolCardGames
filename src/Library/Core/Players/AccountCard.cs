namespace CoolCardGames.Library.Core.Actors;

public record AccountCard(string Id, string DisplayName)
{
    public override string ToString() => $"{DisplayName} ({Id})";
}