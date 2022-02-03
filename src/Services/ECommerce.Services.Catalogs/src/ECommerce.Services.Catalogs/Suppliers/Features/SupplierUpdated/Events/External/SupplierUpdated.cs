using BuildingBlocks.Core.Domain.Events;
using BuildingBlocks.Core.Domain.Events.External;

namespace ECommerce.Services.Catalogs.Suppliers.Features.SupplierUpdated.Events.External;

public record SupplierUpdated(long Id, string Name) : IntegrationEvent;


public class SupplierUpdatedConsumer : IEventHandler<SupplierUpdated>
{
    public Task Handle(SupplierUpdated notification, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
