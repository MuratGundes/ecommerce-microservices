using ECommerce.Services.Catalogs.Suppliers.Data;
using MicroBootstrap.Abstractions.Persistence;

namespace ECommerce.Services.Catalogs.Suppliers;

internal static class Configs
{
    internal static IServiceCollection AddSuppliersServices(this IServiceCollection services)
    {
        services.AddScoped<IDataSeeder, SupplierDataSeeder>();

        return services;
    }

    internal static IEndpointRouteBuilder MapSuppliersEndpoints(this IEndpointRouteBuilder endpoints)
    {
        return endpoints;
    }
}
