using BuildingBlocks.CQRS.Query;

namespace Catalog.Products.Features.GetProductsView;

// GET api/v1/catalog/products
public static class GetProductsViewEndpoint
{
    internal static IEndpointRouteBuilder MapGetProductsViewEndpoint(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
                $"{CatalogConfiguration.CatalogModulePrefixUri}/products-view/{{page}}/{{pageSize}}",
                GetProductsView)
            .WithTags(Configs.Tag)
            // .RequireAuthorization()
            .Produces<GetProductsViewQueryResult>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status400BadRequest)
            .WithDisplayName("Get products.");

        return endpoints;
    }

    private static async Task<IResult> GetProductsView(
        IQueryProcessor queryProcessor,
        CancellationToken cancellationToken,
        int page = 1,
        int pageSize = 20)
    {
        var result = await queryProcessor.SendAsync(
            new GetProductsViewQuery { Page = page, PageSize = pageSize },
            cancellationToken);

        return Results.Ok(result);
    }
}
