using Ardalis.GuardClauses;
using ECommerce.Services.Catalogs.Products.Exceptions.Application;
using ECommerce.Services.Catalogs.Products.Models;
using ECommerce.Services.Catalogs.Shared.Data;
using MicroBootstrap.Abstractions.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Domain.Events.Internal;
using MicroBootstrap.Core.Exception;

namespace ECommerce.Services.Catalogs.Products.Features.UpdatingProduct;

public record ProductUpdated(Product Product) : DomainEvent;

public class ProductUpdatedHandler : IDomainEventHandler<ProductUpdated>
{
    private readonly CatalogDbContext _dbContext;

    public ProductUpdatedHandler(CatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task Handle(ProductUpdated notification, CancellationToken cancellationToken)
    {
        Guard.Against.Null(notification, nameof(notification));

        var existed = await _dbContext.ProductsView
            .FirstOrDefaultAsync(x => x.ProductId == notification.Product.Id, cancellationToken);

        if (existed is not null)
        {
            var product = await _dbContext.Products
                .Include(x => x.Brand)
                .Include(x => x.Category)
                .Include(x => x.Supplier)
                .SingleOrDefaultAsync(x => x.Id == notification.Product.Id, cancellationToken);

            Guard.Against.NotFound(product, new ProductNotFoundException(notification.Product.Id));

            existed.ProductId = product!.Id;
            existed.ProductName = product.Name;
            existed.CategoryId = product.CategoryId;
            existed.CategoryName = product.Category?.Name ?? string.Empty;
            existed.SupplierId = product.SupplierId;
            existed.SupplierName = product.Supplier?.Name ?? string.Empty;
            existed.BrandId = product.BrandId;
            existed.BrandName = product.Brand?.Name ?? string.Empty;

            _dbContext.Set<ProductView>().Update(existed);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
    }
}
