using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace FieldOps.Api.Tests;

public sealed class TestAuthenticationHandler
    : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "Test";

    public TestAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult>
        HandleAuthenticateAsync()
    {
        var role = Request.Headers["X-Test-Role"]
            .FirstOrDefault();

        if (string.IsNullOrWhiteSpace(role))
        {
            return Task.FromResult(
                AuthenticateResult.NoResult());
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                Request.Headers["X-Test-Object-Id"]
                    .FirstOrDefault() ?? "integration-test-user"),

            new(ClaimTypes.Role, role)
        };

        var objectId = Request.Headers["X-Test-Object-Id"]
            .FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(objectId))
        {
            claims.Add(new Claim("oid", objectId));
        }

        var identity = new ClaimsIdentity(
            claims,
            SchemeName,
            ClaimTypes.NameIdentifier,
            ClaimTypes.Role);

        var principal = new ClaimsPrincipal(identity);

        var ticket = new AuthenticationTicket(
            principal,
            SchemeName);

        return Task.FromResult(
            AuthenticateResult.Success(ticket));
    }
}