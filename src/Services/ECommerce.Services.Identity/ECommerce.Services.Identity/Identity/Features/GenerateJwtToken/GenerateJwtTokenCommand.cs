using System.Security.Claims;
using ECommerce.Services.Identity.Shared.Models;
using MicroBootstrap.Abstractions.CQRS.Command;
using MicroBootstrap.Security.Jwt;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace ECommerce.Services.Identity.Identity.Features.GenerateJwtToken;

public record GenerateJwtTokenCommand(ApplicationUser User, string RefreshToken)
    : ICommand<GenerateJwtTokenCommandResult>;

public class GenerateRefreshTokenCommandHandler :
    ICommandHandler<GenerateJwtTokenCommand, GenerateJwtTokenCommandResult>
{
    private readonly IJwtHandler _jwtHandler;
    private readonly ILogger<GenerateRefreshTokenCommandHandler> _logger;
    private readonly UserManager<ApplicationUser> _userManager;

    public GenerateRefreshTokenCommandHandler(UserManager<ApplicationUser> userManager, IJwtHandler jwtHandler,
        ILogger<GenerateRefreshTokenCommandHandler> logger)
    {
        _userManager = userManager;
        _jwtHandler = jwtHandler;
        _logger = logger;
    }

    public async Task<GenerateJwtTokenCommandResult> Handle(GenerateJwtTokenCommand request,
        CancellationToken cancellationToken)
    {
        var identityUser = request.User;

        // authentication successful so generate jwt and refresh tokens
        var allClaims = await GetClaimsAsync(request.User.UserName);
        var fullName = $"{identityUser.FirstName} {identityUser.LastName}";

        var jsonWebToken = _jwtHandler.GenerateJwtToken(identityUser.UserName, identityUser.Email,
            identityUser.Id.ToString(), identityUser.EmailConfirmed || identityUser.PhoneNumberConfirmed, fullName,
            request.RefreshToken, allClaims.UserClaims, allClaims.Roles, allClaims.PermissionClaims);

        _logger.LogInformation("JsonWebToken generated with this information: {JsonWebToken}", jsonWebToken);

        return new GenerateJwtTokenCommandResult(jsonWebToken);
    }

    public async Task<(IList<Claim> UserClaims, IList<string> Roles, IList<string> PermissionClaims)>
        GetClaimsAsync(string userName)
    {
        var appUser = await _userManager.FindByNameAsync(userName);
        var userClaims =
            (await _userManager.GetClaimsAsync(appUser)).Where(x => x.Type != CustomClaimTypes.Permission).ToList();
        var roles = await _userManager.GetRolesAsync(appUser);

        var permissions = (await _userManager.GetClaimsAsync(appUser))
            .Where(x => x.Type == CustomClaimTypes.Permission)?.Select(x => x
                .Value).ToList();

        return (UserClaims: userClaims, Roles: roles, PermissionClaims: permissions);
    }
}
