using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.GameEventUtils;

public class ChannelGameEventPublisherFactory(ILogger<ChannelGameEventPublisher> logger)
{
    public ChannelGameEventPublisher Make(ChannelWriter<GameEventEnvelope> writer)
    {
        return new ChannelGameEventPublisher(writer, logger);
    }
}