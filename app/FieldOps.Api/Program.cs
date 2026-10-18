using FieldOps.Api.Data;
using FieldOps.Api.Models;
using Microsoft.EntityFrameworkCore;
using FieldOps.Api.Services;
using FieldOps.Api.Authorization;
using Microsoft.AspNetCore.Authorization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<FieldOpsDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("FieldOps")));

builder.Services.AddControllers();
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        FieldOpsPolicies.Customer,
        policy => policy.RequireRole(FieldOpsPolicies.Customer));

    options.AddPolicy(
        FieldOpsPolicies.Technician,
        policy => policy.RequireRole(FieldOpsPolicies.Technician));

    options.AddPolicy(
        FieldOpsPolicies.Dispatcher,
        policy => policy.RequireRole(FieldOpsPolicies.Dispatcher));

    options.AddPolicy(
        FieldOpsPolicies.Administrator,
        policy => policy.RequireRole(FieldOpsPolicies.Administrator));

    options.AddPolicy(
        FieldOpsPolicies.CustomerOrDispatcherOrAdministrator,
        policy => policy.RequireRole(
            FieldOpsPolicies.Customer,
            FieldOpsPolicies.Dispatcher,
            FieldOpsPolicies.Administrator));

    options.AddPolicy(
        FieldOpsPolicies.TechnicianOrDispatcherOrAdministrator,
        policy => policy.RequireRole(
            FieldOpsPolicies.Technician,
            FieldOpsPolicies.Dispatcher,
            FieldOpsPolicies.Administrator));

    options.AddPolicy(
        FieldOpsPolicies.DispatcherOrAdministrator,
        policy => policy.RequireRole(
            FieldOpsPolicies.Dispatcher,
            FieldOpsPolicies.Administrator));
});
builder.Services.AddHealthChecks();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IFieldOpsAuthorizationService, FieldOpsAuthorizationService>();
builder.Services.AddScoped<IUserIdentityService, UserIdentityService>();
builder.Services.AddScoped<IAttachmentStorage, LocalAttachmentStorage>();
builder.Services.Configure<AttachmentStorageOptions>(
    builder.Configuration.GetSection("AttachmentStorage"));

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();

    var db = scope.ServiceProvider.GetRequiredService<FieldOpsDbContext>();

    db.Database.Migrate();

    if (!db.Customers.Any())
    {
        db.Customers.AddRange(
            new Customer
            {
                Name = "ABC Services",
                Email = "operations@abcservices.example",
                Phone = "+91-9000000001"
            },
            new Customer
            {
                Name = "Kerala Facilities Ltd.",
                Email = "facilities@keralafacilities.example",
                Phone = "+91-9000000002"
            });

        db.SaveChanges();
    }

    if (!db.Technicians.Any())
    {
        db.Technicians.AddRange(
            new Technician
            {
                Name = "Arun Kumar",
                Email = "arun.kumar@fieldops.example",
                Phone = "+91-9000000011"
            },
            new Technician
            {
                Name = "Suresh Nair",
                Email = "suresh.nair@fieldops.example",
                Phone = "+91-9000000012"
            },
            new Technician
            {
                Name = "Rahul Menon",
                Email = "rahul.menon@fieldops.example",
                Phone = "+91-9000000013"
            });

        db.SaveChanges();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapHealthChecks("/health");
app.MapControllers();

app.MapGet("/", () => "FieldOps API");

app.Run();

public partial class Program { }