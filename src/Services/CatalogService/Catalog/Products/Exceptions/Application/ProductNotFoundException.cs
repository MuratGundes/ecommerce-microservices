using BuildingBlocks.Exception;

namespace Catalog.Products.Exceptions.Application;

public class ProductNotFoundException : NotFoundException
{
    public ProductNotFoundException(long id) : base($"Product with id '{id}' not found")
    {
    }

    public ProductNotFoundException(string message) : base(message)
    {
    }
}
