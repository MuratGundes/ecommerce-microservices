using System.Threading.Channels;
using BuildingBlocks.Core.Domain.Events.External;

namespace BuildingBlocks.Messaging.Transport.InMemory.Channels;

public interface IMessageChannel
{
    ChannelReader<IIntegrationEvent> Reader { get; }
    ChannelWriter<IIntegrationEvent> Writer { get; }
}
