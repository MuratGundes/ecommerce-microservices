using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Events.External;

namespace Catalog.Products.Features.CreatingProduct.Events.Integration;

public record ProductCreated(long Id, string Name, long CategoryId, string CategoryName, int Stock) :
    IntegrationEvent;

public class ProductCreatedConsumer :
    IIntegrationEventHandler<ProductCreated>,
    IIntegrationEventHandler<IntegrationEventWrapper<Events.Domain.ProductCreated>>
{

    public ProductCreatedConsumer()
    {
    }

    public Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));
        return Task.CompletedTask;
    }

    public Task Handle(IntegrationEventWrapper<Events.Domain.ProductCreated> notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));
        return Task.CompletedTask;
    }
}
