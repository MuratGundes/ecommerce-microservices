using Ardalis.GuardClauses;
using AutoMapper;
using BuildingBlocks.CQRS.Command;
using Catalog.Products.Features.CreatingProduct.Requests;
using Catalog.Products.Infrastructure;

namespace Catalog.Products.Features.CreatingProduct;

// POST api/v1/catalog/products
public static class CreateProductEndpoint
{
    internal static IEndpointRouteBuilder MapCreateProductsEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost($"{CatalogConfiguration.CatalogModulePrefixUri}{ProductsConfigs.ProductsPrefixUri}", CreateProducts)
            .WithTags(ProductsConfigs.Tag)
            // .RequireAuthorization()
            .Produces<CreateProductResult>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithName("CreateProduct")
            .WithDisplayName("Create a new product.");

        return endpoints;
    }

    private static async Task<IResult> CreateProducts(
        CreateProductRequest request,
        ICommandProcessor commandProcessor,
        IMapper mapper,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));
        var command = mapper.Map<CreateProduct>(request);
        var result = await commandProcessor.SendAsync(command, cancellationToken);

        return Results.CreatedAtRoute("GetProductById", new { id = result.Product.Id }, result);
    }
}
