using Ardalis.GuardClauses;
using BuildingBlocks.Core.Domain.Model;

namespace ECommerce.Services.Customers.RestockSubscriptions.ValueObjects;

public class ProductId : AggregateId<long>
{
    public ProductId(long value) : base(value)
    {
    }

    public static implicit operator long(ProductId id) => Guard.Against.Null(id.Value, nameof(id.Value));

    public static implicit operator ProductId(long id) => new(id);
}
