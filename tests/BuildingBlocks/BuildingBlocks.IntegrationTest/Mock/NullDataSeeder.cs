using MicroBootstrap.Abstractions.Persistence;

namespace BuildingBlocks.IntegrationTests.Mock;

public class NullDataSeeder : IDataSeeder
{
    public Task SeedAllAsync()
    {
        return Task.CompletedTask;
    }
}
