using System.Security.Claims;
using FieldOps.Api.Data;
using FieldOps.Api.Models;
using FieldOps.Api.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace FieldOps.Api.Tests;

public class FieldOpsAuthorizationServiceTests
{
    [Fact]
    public async Task Customer_CanAccessOwnServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            fixture.OwnCustomer.IdentityObjectId!,
            "Customer");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task Customer_CannotAccessAnotherCustomersServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            fixture.OtherCustomer.IdentityObjectId!,
            "Customer");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task AssignedTechnician_CanAccessServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            fixture.Technician.IdentityObjectId!,
            "Technician");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task Technician_CannotAccessUnassignedServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            fixture.OtherTechnician.IdentityObjectId!,
            "Technician");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.False(result);
    }

    [Fact]
    public async Task Dispatcher_CanAccessAnyExistingServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            "dispatcher-001",
            "Dispatcher");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task Administrator_CanAccessAnyExistingServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            "administrator-001",
            "Administrator");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.True(result);
    }

    [Fact]
    public async Task MissingIdentity_CannotAccessServiceRequest()
    {
        await using var fixture =
            await AuthorizationTestFixture.CreateAsync();

        fixture.SetUser(
            null,
            "Customer");

        var result =
            await fixture.AuthorizationService
                .CanAccessServiceRequestAsync(
                    fixture.OwnRequest.Id);

        Assert.False(result);
    }

    private sealed class TestHttpContextAccessor
        : IHttpContextAccessor
    {
        public HttpContext? HttpContext { get; set; }
    }

    private sealed class AuthorizationTestFixture
        : IAsyncDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly FieldOpsDbContext _db;
        private readonly DefaultHttpContext _httpContext;
        private readonly TestHttpContextAccessor _httpContextAccessor;

        public FieldOpsAuthorizationService AuthorizationService
        {
            get;
        }

        public Customer OwnCustomer { get; private set; } = null!;

        public Customer OtherCustomer { get; private set; } = null!;

        public Technician Technician { get; private set; } = null!;

        public Technician OtherTechnician { get; private set; } = null!;

        public ServiceRequest OwnRequest { get; private set; } = null!;

        private AuthorizationTestFixture(
            SqliteConnection connection,
            FieldOpsDbContext db,
            DefaultHttpContext httpContext)
        {
            _connection = connection;
            _db = db;
            _httpContext = httpContext;

            _httpContextAccessor =
                new TestHttpContextAccessor
                {
                    HttpContext = _httpContext
                };

            var userIdentityService =
                new UserIdentityService(
                    _httpContextAccessor);

            AuthorizationService =
                new FieldOpsAuthorizationService(
                    _db,
                    userIdentityService,
                    _httpContextAccessor);
        }

        public static async Task<AuthorizationTestFixture>
            CreateAsync()
        {
            var connection =
                new SqliteConnection(
                    "Data Source=:memory:");

            await connection.OpenAsync();

            var options =
                new DbContextOptionsBuilder<FieldOpsDbContext>()
                    .UseSqlite(connection)
                    .Options;

            var db =
                new FieldOpsDbContext(options);

            await db.Database.EnsureCreatedAsync();

            var httpContext =
                new DefaultHttpContext();

            var fixture =
                new AuthorizationTestFixture(
                    connection,
                    db,
                    httpContext);

            await fixture.SeedAsync();

            return fixture;
        }

        private async Task SeedAsync()
        {
            OwnCustomer =
                new Customer
                {
                    Name = "Own Customer",
                    Email = "own@example.com",
                    IdentityObjectId = "customer-own-001",
                    Phone = "1111111111"
                };

            OtherCustomer =
                new Customer
                {
                    Name = "Other Customer",
                    Email = "other@example.com",
                    IdentityObjectId = "customer-other-001",
                    Phone = "2222222222"
                };

            Technician =
                new Technician
                {
                    Name = "Assigned Technician",
                    Email = "tech@example.com",
                    IdentityObjectId = "technician-001",
                    Phone = "3333333333",
                    IsActive = true
                };

            OtherTechnician =
                new Technician
                {
                    Name = "Other Technician",
                    Email = "othertech@example.com",
                    IdentityObjectId = "technician-002",
                    Phone = "4444444444",
                    IsActive = true
                };

            await _db.Customers.AddRangeAsync(
                OwnCustomer,
                OtherCustomer);

            await _db.Technicians.AddRangeAsync(
                Technician,
                OtherTechnician);

            await _db.SaveChangesAsync();

            OwnRequest =
                new ServiceRequest
                {
                    CustomerId = OwnCustomer.Id,
                    Description = "Test service request",
                    Status = "New"
                };

            var otherRequest =
                new ServiceRequest
                {
                    CustomerId = OtherCustomer.Id,
                    Description = "Other service request",
                    Status = "New"
                };

            await _db.ServiceRequests.AddRangeAsync(
                OwnRequest,
                otherRequest);

            await _db.SaveChangesAsync();

            var assignment =
                new Assignment
                {
                    ServiceRequestId = OwnRequest.Id,
                    TechnicianId = Technician.Id,
                    AssignedAtUtc = DateTime.UtcNow,
                    IsActive = true
                };

            await _db.Assignments.AddAsync(
                assignment);

            await _db.SaveChangesAsync();
        }

        public void SetUser(
            string? identityObjectId,
            string role)
        {
            var claims =
                new List<Claim>
                {
                    new Claim(
                        ClaimTypes.Role,
                        role)
                };

            if (!string.IsNullOrWhiteSpace(
                    identityObjectId))
            {
                claims.Add(
                    new Claim(
                        "oid",
                        identityObjectId));
            }

            var identity =
                new ClaimsIdentity(
                    claims,
                    authenticationType: "Test",
                    nameType: ClaimTypes.Name,
                    roleType: ClaimTypes.Role);

            _httpContext.User =
                new ClaimsPrincipal(identity);

            _httpContextAccessor.HttpContext =
                _httpContext;
        }

        public async ValueTask DisposeAsync()
        {
            await _db.DisposeAsync();
            await _connection.DisposeAsync();
        }
    }
}