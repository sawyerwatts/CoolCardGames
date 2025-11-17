namespace CoolCardGames.Library.Core.GameSessionExceptions;

public class FailedToApplyGameEventToSessionException : Exception
{
    public GameEventEnvelope EnvelopeNotRendered { get; }

    public FailedToApplyGameEventToSessionException(GameEventEnvelope envelopeNotRendered)
    {
        EnvelopeNotRendered = envelopeNotRendered;
    }

    public FailedToApplyGameEventToSessionException(GameEventEnvelope envelopeNotRendered, string message) : base(message)
    {
        EnvelopeNotRendered = envelopeNotRendered;
    }

    public FailedToApplyGameEventToSessionException(GameEventEnvelope envelopeNotRendered, string message, Exception innerException) : base(message, innerException)
    {
        EnvelopeNotRendered = envelopeNotRendered;
    }
}