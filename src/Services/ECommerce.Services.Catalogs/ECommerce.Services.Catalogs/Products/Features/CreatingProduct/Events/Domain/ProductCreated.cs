using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Abstractions.Messaging.Outbox;
using MicroBootstrap.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Exception;

namespace ECommerce.Services.Catalogs.Products.Features.CreatingProduct.Events.Domain;

// , IHaveNotificationEvent, IHaveExternalEvent; --> For automatic integration event creating without creating an explicit integration event class
public record ProductCreated(Product Product) : DomainEvent;

internal class ProductCreatedHandler : IDomainEventHandler<ProductCreated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductCreatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductCreated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var existed = await _dbContext.ProductsView
            .SingleOrDefaultAsync(x => x.ProductId == notification.Product.Id.Value, cancellationToken);

        if (existed is null)
        {
            var product = await _dbContext.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Product.Id.Value, cancellationToken);

            Guard.Against.NotFound(product, new ProductNotFoundException(notification.Product.Id));

            var productView = new ProductView
            {
                ProductId = product!.Id,
                ProductName = product.Name,
                CategoryId = product.CategoryId,
                CategoryName = product.Category?.Name ?? string.Empty,
                SupplierId = product.SupplierId,
                SupplierName = product.Supplier?.Name ?? string.Empty,
                BrandId = product.BrandId,
                BrandName = product.Brand?.Name ?? string.Empty,
            };

            await _dbContext.Set<ProductView>().AddAsync(productView, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}

// Mapping domain event to integration event in domain event handler is better from mapping in command handler (for preserving our domain rule invariants).
internal class ProductCreatedDomainEventToIntegrationMappingHandler : IDomainEventHandler<ProductCreated>
{
    private readonly IOutboxService _outboxService;

    public ProductCreatedDomainEventToIntegrationMappingHandler(IOutboxService outboxService)
    {
        _outboxService = outboxService;
    }

    public Task Handle(ProductCreated domainEvent, CancellationToken cancellationToken)
    {
        // 1. Mapping DomainEvent To IntegrationEvent
        // 2. Save Integration Event to Outbox
        return Task.CompletedTask;
    }
}
