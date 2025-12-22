using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

namespace BlazorPlayground.FunctionalTests;

public sealed class ApiFactory : WebApplicationFactory<BlazorPlayground.API.Program>/*, IAsyncLifetime*/
{
    //private readonly IHost _app;
    
    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureHostConfiguration(config =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                { "ConnectionStrings:DefaultConnection", "Server=localhost,1433;Database=BlazorPlayground;User Id=sa;Password=@ndrewG88;TrustServerCertificate=True;" },
                { "Jwt:Issuer", "test-issuer" },
                { "Jwt:Audience", "blazor-dev" },
                { "Jwt:Secret", "Cw7RZp4tPq9Kf2LxM8bN3yV6sT1Qz8Hd" },
            });
        });
        
        builder.ConfigureServices(services =>
        {
            services.AddSingleton<JwtFactory>();
            services.AddSingleton<IStartupFilter>(new AutoAuthorizeStartupFilter());
            
            services.PostConfigure<JwtBearerOptions>("Bearer", o =>
            {
                o.Authority = null;
                o.MetadataAddress = null;
                o.ConfigurationManager = null;

                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = "test-issuer",
                    ValidateAudience = true,
                    ValidAudience = "blazor-dev",
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes("Cw7RZp4tPq9Kf2LxM8bN3yV6sT1Qz8Hd")),
                    ValidAlgorithms = new[] { SecurityAlgorithms.HmacSha256 },
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
        });
        return base.CreateHost(builder);
    }

    /*public async Task InitializeAsync()
    {
        await _app.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await base.DisposeAsync();
        await _app.StopAsync();
        if (_app is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync().ConfigureAwait(false);
        }
        else
        {
            _app.Dispose();
        }
    }*/
    
    private class AutoAuthorizeStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return builder =>
            {
                builder.UseMiddleware<AutoAuthorizeMiddleware>();
                next(builder);
            };
        }
    }
}