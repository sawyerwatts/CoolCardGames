using System.Collections.Concurrent;
using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.MiscUtils;

/// <inheritdoc cref="HandleFanOut"/>
/// <remarks>
/// It is intended to use <see cref="ChannelFanOutFactory"/> to instantiate this service.
/// </remarks>
public class ChannelFanOut<TMessage>(
    ChannelReader<TMessage> sourceReader,
    ILogger<ChannelFanOut<TMessage>> logger)
{
    private readonly ConcurrentBag<Destination> _destinations = [];

    public bool Completed { get; private set; } = false;

    public ChannelReader<TMessage> CreateReader(string name, bool singleReader = true)
    {
        if (_destinations.Any(destination => destination.Name == name))
            throw new ArgumentException($"There is already a destination with name {name}");

        var channel = Channel.CreateUnbounded<TMessage>(new UnboundedChannelOptions() { SingleWriter = true, SingleReader = singleReader, });
        var destination = new Destination(name, channel);
        _destinations.Add(destination);
        return destination.Channel.Reader;
    }

    /// <summary>
    /// This will fanout/forward all messages from <see cref="sourceReader"/> to all the created
    /// destinations (via <see cref="CreateReader"/>) until <see cref="sourceReader"/> has been
    /// completed.
    /// <br />
    /// When the source channel is completed, the destination channels will be completed too, and
    /// then this method will complete.
    /// </summary>
    /// <param name="cancellationToken"></param>
    public async Task HandleFanOut(CancellationToken cancellationToken)
    {
        Exception? exc = null;
        try
        {
            logger.LogInformation("Beginning fan out");
            await foreach (var msg in sourceReader.ReadAllAsync(cancellationToken))
            {
                logger.LogDebug("Fanning out message: {Message}", msg);
                foreach (var destination in _destinations)
                {
                    logger.LogDebug("Fanning out message to destination {DestinationName}", destination.Name);
                    await destination.Channel.Writer.WriteAsync(msg, cancellationToken);
                    logger.LogDebug("Fanned out message to destination {DestinationName}", destination.Name);
                }

                logger.LogDebug("Fanned out message");
            }

            logger.LogInformation("Ending fan out");
        }
        catch (Exception e)
        {
            exc = e;
        }
        finally
        {
            CompleteDestinations(exc);
        }
    }

    private void CompleteDestinations(Exception? exc = null)
    {
        if (exc is null)
            logger.LogInformation("Completing destination channels");
        else
            logger.LogError(exc, "Completing destination channels");

        var anyCompletionExceptions = false;
        foreach (var destination in _destinations)
        {
            try
            {
                destination.Channel.Writer.Complete(exc);
            }
            catch (Exception e)
            {
                logger.LogError(e, "Could not complete the destination channel for {DestinationName}", destination.Name);
                anyCompletionExceptions = true;
            }
        }

        if (anyCompletionExceptions)
        {
            logger.LogError("Could not complete all the channels");
            throw new InvalidOperationException("Could not complete all the channels; see the log for more");
        }

        logger.LogInformation("Completed all the destination channels");
        Completed = true;
    }

    protected readonly record struct Destination(string Name, Channel<TMessage> Channel);
}