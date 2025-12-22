using System.Security.Claims;
using BlazorPlayground.API;
using BlazorPlayground.API.Infrastructure;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddMigration<AppDbContext>();

builder.Services.AddAuthentication(o=>
    {
        o.DefaultAuthenticateScheme = "Bearer";
        o.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", jwtOptions =>
    {
        jwtOptions.RequireHttpsMetadata = false;
        
        jwtOptions.Authority = "http://localhost:5556/dex";
        
        jwtOptions.Audience = "blazor-dev";
    });

builder.Services.AddAuthorization(options =>
{
    /*options.AddPolicy("Customers.Read", policy =>
        policy.RequireAssertion(ctx => HasScope(ctx.User, "customers.read")));

    options.AddPolicy("Customers.Write", policy =>
        policy.RequireAssertion(ctx => HasScope(ctx.User, "customers.write")));*/
});

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// Add Endpoints API Explorer
builder.Services.AddEndpointsApiExplorer();

// Add NSwag services
builder.Services.AddOpenApiDocument();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    
    // Add OpenAPI/Swagger generator and the Swagger UI
    app.UseOpenApi();
    app.UseSwaggerUi();
}

app.UseHttpsRedirection();

app.MapCustomerEndpoints();

app.Run();
return;

static bool HasScope(ClaimsPrincipal user, string scope)
{
    // Common claim types:
    // - "scope" (space-separated)
    // - "scp"   (Azure-style, also space-separated)
    var scopes =
        user.FindAll("scope").SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries))
            .Concat(user.FindAll("scp").SelectMany(c => c.Value.Split(' ', StringSplitOptions.RemoveEmptyEntries)));

    return scopes.Contains(scope, StringComparer.OrdinalIgnoreCase);
}