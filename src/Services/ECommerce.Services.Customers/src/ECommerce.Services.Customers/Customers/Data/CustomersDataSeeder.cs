using BuildingBlocks.Core.Persistence;

namespace ECommerce.Services.Customers.Customers.Data;

public class CustomersDataSeeder : IDataSeeder
{
    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
