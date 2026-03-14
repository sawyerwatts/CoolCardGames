namespace CoolCardGames.Library.Core.MiscUtils;

public sealed class Disposable(Action dispose) : IDisposable
{
    public void Dispose() => dispose();
}