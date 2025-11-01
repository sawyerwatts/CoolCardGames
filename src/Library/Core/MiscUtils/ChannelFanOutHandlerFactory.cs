using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.MiscUtils;

public class ChannelFanOutHandlerFactory(ILoggerFactory loggerFactory)
{
    public ChannelFanOutHandler<TMessage> Make<TMessage>(ChannelReader<TMessage> sourceReader)
    {
        var logger = loggerFactory.CreateLogger<ChannelFanOutHandler<TMessage>>();
        return new ChannelFanOutHandler<TMessage>(sourceReader, logger);
    }
}