
using BlazorPlayground.WebApp;
using BlazorPlayground.WebApp.Client.Services;
using BlazorPlayground.WebApp.Components;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using CustomerClient = BlazorPlayground.WebApp.Services.CustomerClient;

const string OIDC_SCHEME = "Oidc";

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAuthentication(OIDC_SCHEME)
    .AddOpenIdConnect(OIDC_SCHEME, oidcOptions =>
    {
        oidcOptions.RequireHttpsMetadata = false;
        
        // For the following OIDC settings, any line that's commented out
        // represents a DEFAULT setting. If you adopt the default, you can
        // remove the line if you wish.

        //oidcOptions.PushedAuthorizationBehavior = PushedAuthorizationBehavior.UseIfAvailable;
       
        oidcOptions.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        
        oidcOptions.Scope.Add(OpenIdConnectScope.OpenIdProfile);

        //oidcOptions.CallbackPath = new PathString("/signin-oidc");
        //oidcOptions.SignedOutCallbackPath = new PathString("/signout-callback-oidc");
       
        //oidcOptions.RemoteSignOutPath = new PathString("/signout-oidc");

        oidcOptions.Authority = "http://localhost:5556/dex";
    
        oidcOptions.ClientId = "blazor-dev";
        oidcOptions.ClientSecret = "blazor-secret";
       
        oidcOptions.ResponseType = OpenIdConnectResponseType.Code;
        
        oidcOptions.MapInboundClaims = false;
        oidcOptions.TokenValidationParameters.NameClaimType = "name";
        oidcOptions.TokenValidationParameters.RoleClaimType = "groups"; //"roles";
        
        // handle end session endpoint missing feature in dex
        oidcOptions.Events = new OpenIdConnectEvents()
        {
            OnRedirectToIdentityProviderForSignOut = context =>
            {
                // If the OP doesn't advertise an end-session endpoint (Dex), don't blow up.
                var endSession = context.Options.Configuration?.EndSessionEndpoint;
                if (string.IsNullOrWhiteSpace(endSession))
                {
                    context.HandleResponse(); // tell middleware "I'll handle the response"
                    context.Response.Redirect("/"); // go wherever you want after logout
                }

                return Task.CompletedTask;
            }

        };
    })
    .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme);

builder.Services.ConfigureCookieOidc(CookieAuthenticationDefaults.AuthenticationScheme, OIDC_SCHEME);

builder.Services.AddAuthorization();

builder.Services.AddCascadingAuthenticationState();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(options => options.SerializeAllClaims = true);

builder.Services.AddScoped<AuthenticationStateProvider, PersistingAuthenticationStateProvider>();

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<TokenHandler>();

builder.Services.AddHttpClient("API",
        client => client.BaseAddress = new Uri(builder.Configuration["BackendUrl"] ?? "https://localhost:7020"))
    .AddHttpMessageHandler<TokenHandler>();

builder.Services.AddScoped<ICustomerClient, BlazorPlayground.WebApp.Services.CustomerClient>(sp=>
{
    var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
    var httpClient = httpClientFactory.CreateClient("API");
    return new CustomerClient(httpClient);
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(BlazorPlayground.WebApp.Client._Imports).Assembly);

app.MapGroup("/authentication").MapLoginAndLogout();
app.MapCustomerEndpoints();



app.Run();