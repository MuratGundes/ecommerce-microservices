using ECommerce.Services.Identity.Identity.Features.GetClaims;
using ECommerce.Services.Identity.Identity.Features.Login;
using ECommerce.Services.Identity.Identity.Features.Logout;
using ECommerce.Services.Identity.Identity.Features.RefreshingToken;
using ECommerce.Services.Identity.Identity.Features.RevokeRefreshToken;
using ECommerce.Services.Identity.Identity.Features.SendEmailVerificationCode;
using ECommerce.Services.Identity.Identity.Features.VerifyEmail;
using ECommerce.Services.Identity.Shared.Data;
using ECommerce.Services.Identity.Shared.Extensions.ServiceCollectionExtensions;
using MicroBootstrap.Abstractions.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ECommerce.Services.Identity.Identity;

internal static class IdentityConfigs
{
    public const string Tag = "Identity";
    public const string IdentityPrefixUri = $"{IdentityModuleConfiguration.IdentityModulePrefixUri}";

    internal static IServiceCollection AddIdentityServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IDataSeeder, IdentityDataSeeder>();
        services.AddCustomIdentity(configuration);
        services.AddCustomIdentityServer();

        return services;
    }

    internal static IEndpointRouteBuilder MapIdentityEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet(
            $"{IdentityPrefixUri}/user-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = Constants.Role.User)]
            () => new { Role = Constants.Role.User }).WithTags("Identity");

        endpoints.MapGet(
            $"{IdentityPrefixUri}/admin-role",
            [Authorize(
                AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
                Roles = Constants.Role.Admin)]
            () => new { Role = Constants.Role.Admin }).WithTags("Identity");

        endpoints.MapLoginUserEndpoint();
        endpoints.MapLogoutEndpoint();
        endpoints.MapSendEmailVerificationCodeEndpoint();
        endpoints.MapSendVerifyEmailEndpoint();
        endpoints.MapRefreshTokenEndpoint();
        endpoints.MapRevokeTokenEndpoint();
        endpoints.MapGetClaimsEndpoint();

        return endpoints;
    }
}
