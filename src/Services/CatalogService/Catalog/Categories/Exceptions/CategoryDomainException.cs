using BuildingBlocks.Domain.Exceptions;

namespace Catalog.Categories.Exceptions;

public class CategoryDomainException : DomainException
{
    public CategoryDomainException(string message) : base(message)
    {
    }

    public CategoryDomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
