using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace BlazorPlayground.FunctionalTests;

class AutoAuthorizeMiddleware
{
    public const string IDENTITY_ID = "9e3163b9-1ae6-4652-9dc6-7898ab7b7a00";

    private readonly RequestDelegate _next;

    public AutoAuthorizeMiddleware(RequestDelegate rd)
    {
        _next = rd;
    }

    public async Task Invoke(HttpContext httpContext, JwtFactory jwtFactory)
    {
        /*var identity = new ClaimsIdentity([
            new Claim("sub", IDENTITY_ID),
            new Claim("unique_name", IDENTITY_ID),
            new Claim(ClaimTypes.Name, IDENTITY_ID)
        ], "cookies");

        //httpContext.User = new ClaimsPrincipal(identity);
        
        httpContext.User.AddIdentity(identity);
        
        var userId = Guid.Parse(IDENTITY_ID);*/
        var token = jwtFactory.Create();

        httpContext.Request.Headers.Authorization = "Bearer " + token;

        await _next.Invoke(httpContext);
    }
}