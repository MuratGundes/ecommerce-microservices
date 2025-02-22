using ECommerce.Services.Customers.Customers.Models.Reads;
using ECommerce.Services.Customers.RestockSubscriptions.Models.Read;
using Humanizer;
using MicroBootstrap.Persistence.Mongo;
using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace ECommerce.Services.Customers.Shared.Data;

public class CustomersReadDbContext : MongoDbContext
{
    public CustomersReadDbContext(IOptions<MongoOptions> options) : base(options.Value)
    {
        RestockSubscriptions = GetCollection<RestockSubscriptionReadModel>(nameof(RestockSubscriptions).Underscore());
        Customers = GetCollection<CustomerReadModel>(nameof(Customers).Underscore());
    }

    public IMongoCollection<RestockSubscriptionReadModel> RestockSubscriptions { get; }
    public IMongoCollection<CustomerReadModel> Customers { get; }
}
