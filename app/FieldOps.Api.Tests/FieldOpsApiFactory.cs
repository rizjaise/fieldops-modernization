using System.Data.Common;
using FieldOps.Api.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using FieldOps.Api.Services;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FieldOps.Api.Tests;

public sealed class FieldOpsApiFactory
    : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            // Remove the production SQL Server provider configuration.
            var dbContextDescriptors = services
    .Where(descriptor =>
        descriptor.ServiceType ==
            typeof(DbContextOptions<FieldOpsDbContext>) ||
        descriptor.ServiceType ==
            typeof(FieldOpsDbContext))
    .ToArray();
            services.RemoveAll<IAttachmentStorage>();

            services.AddSingleton<InMemoryAttachmentStorage>();

            services.AddSingleton<IAttachmentStorage>(
                serviceProvider =>
                    serviceProvider.GetRequiredService<InMemoryAttachmentStorage>());

            foreach (var descriptor in dbContextDescriptors)
            {
                services.Remove(descriptor);
            }

            // Keep one SQLite in-memory connection open for the test host.
            var connectionRegistrations = services
                .Where(descriptor =>
                    descriptor.ServiceType == typeof(DbConnection))
                .ToArray();

            foreach (var descriptor in connectionRegistrations)
            {
                services.Remove(descriptor);
            }

            services.AddSingleton<DbConnection>(_ =>
            {
                var connection =
                    new SqliteConnection("Data Source=:memory:");

                connection.Open();
                return connection;
            });

            services.AddDbContext<FieldOpsDbContext>(
                (serviceProvider, options) =>
                {
                    var connection =
                        serviceProvider.GetRequiredService<DbConnection>();

                    options.UseSqlite(connection);
                });

            // Register the test-only authentication scheme.
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultChallengeScheme =
                        TestAuthenticationHandler.SchemeName;

                    options.DefaultForbidScheme =
                        TestAuthenticationHandler.SchemeName;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthenticationHandler>(
                    TestAuthenticationHandler.SchemeName,
                    _ => { });
        });
    }

    public void EnsureDatabaseCreated()
    {
        using var scope = Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<FieldOpsDbContext>();

        db.Database.EnsureCreated();
    }
}