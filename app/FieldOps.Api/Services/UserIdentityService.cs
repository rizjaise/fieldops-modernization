using System.Security.Claims;

namespace FieldOps.Api.Services;

public class UserIdentityService : IUserIdentityService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserIdentityService(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string? GetIdentityObjectId()
    {
        return _httpContextAccessor.HttpContext?
            .User
            .FindFirstValue("http://schemas.microsoft.com/identity/claims/objectidentifier")
            ?? _httpContextAccessor.HttpContext?
                .User
                .FindFirstValue("oid");
    }
}