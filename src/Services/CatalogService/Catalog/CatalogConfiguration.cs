
using Catalog.Brands;
using Catalog.Categories;
using Catalog.Categories.Data;
using Catalog.Products;
using Catalog.Products.Features.GetProducts;
using Catalog.Products.Features.GetProductsView;
using Catalog.Suppliers;

namespace Catalog;

public static class CatalogConfiguration
{
    public const string CatalogModulePrefixUri = "api/v1/catalog";

    public static WebApplicationBuilder AddCatalogModule(this WebApplicationBuilder builder)
    {
        AddCatalogModule(builder.Services, builder.Configuration);

        return builder;
    }

    public static IServiceCollection AddCatalogModule(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddInfrastructure(configuration);

        services.AddStorage(configuration);

        services.AddCategories();
        services.AddProducts();
        services.AddSuppliers();
        services.AddBrands();

        return services;
    }

    public static IEndpointRouteBuilder MapCatalogModuleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/", () => "Catalog Service Apis").ExcludeFromDescription();
        endpoints.MapGetProductsViewEndpoint();
        endpoints.MapGetProductsEndpoint();

        return endpoints;
    }

    public static async Task ConfigureCatalogModule(
        this IApplicationBuilder app,
        IWebHostEnvironment environment,
        ILogger logger)
    {
        await app.ApplyDatabaseMigrations(logger);
        await app.SeedData(logger, environment);
    }
}
