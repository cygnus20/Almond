using System.Security.Claims;

namespace Almond.API.Core;

public class GetUserClaims(IHttpContextAccessor contextAccessor) : IGetUserClaims
{
    public string? UserId => _contextAccessor?.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

    private readonly IHttpContextAccessor _contextAccessor = contextAccessor;
}
