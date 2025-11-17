namespace CoolCardGames.Library.Core.GameSessionExceptions;

/// <summary>
/// This is thrown when attempting to connect to a running game but the game is not available to attach.
/// </summary>
public class NoCurrentGameToAttachException : Exception
{
    public NoCurrentGameToAttachException()
    {
    }

    public NoCurrentGameToAttachException(string message) : base(message)
    {
    }

    public NoCurrentGameToAttachException(string message, Exception innerException) : base(message, innerException)
    {
    }
}