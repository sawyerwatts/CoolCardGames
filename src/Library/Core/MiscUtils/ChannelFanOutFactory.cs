using System.Threading.Channels;

using Microsoft.Extensions.Logging;

namespace CoolCardGames.Library.Core.MiscUtils;

public class ChannelFanOutFactory(ILoggerFactory loggerFactory)
{
    public ChannelFanOut<TMessage> Make<TMessage>(ChannelReader<TMessage> sourceReader)
    {
        var logger = loggerFactory.CreateLogger<ChannelFanOut<TMessage>>();
        return new ChannelFanOut<TMessage>(sourceReader, logger);
    }
}