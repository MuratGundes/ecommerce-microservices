using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Products.ValueObjects;
using ECommerce.Services.Catalogs.Shared.Contracts;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events.Internal;

namespace ECommerce.Services.Catalogs.Products.Features.DebitingProductStock.Events.Domain;

public record ProductRestockThresholdReachedEvent(ProductId ProductId, Stock Stock, int Quantity) : DomainEvent;

internal class ProductRestockThresholdReachedEventHandler : IDomainEventHandler<ProductRestockThresholdReachedEvent>
{
    private readonly ICatalogDbContext _catalogDbContext;

    public ProductRestockThresholdReachedEventHandler(ICatalogDbContext catalogDbContext)
    {
        _catalogDbContext = catalogDbContext;
    }

    public Task Handle(ProductRestockThresholdReachedEvent notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        // For example send an email to get more products
        return Task.CompletedTask;
    }
}
