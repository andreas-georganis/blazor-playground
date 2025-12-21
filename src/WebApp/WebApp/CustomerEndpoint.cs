using BlazorPlayground.Contracts;
using BlazorPlayground.WebApp.Client.Services;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlazorPlayground.WebApp;

internal static class CustomerEndpoint
{
    internal static IEndpointConventionBuilder MapCustomerEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("webapp/customers")
            .WithTags("Customers").RequireAuthorization();

        group.MapGet("/", async (ICustomerClient customerClient, int page = 1, int pageSize = 10) =>
        {
            var customers = await customerClient.Get(page, pageSize);
            return TypedResults.Ok(customers);
        });
        
        group.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResource>, NotFound>> (Guid id, ICustomerClient customerClient) =>
        {
            var customer = await customerClient.Get(id);

            return customer is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(customer);
        });
        
        group.MapPost("/", async Task<Created<CustomerResource>> (CustomerData customer, ICustomerClient customerClient) =>
        {
            var createdCustomer = await customerClient.Create(customer);

            return TypedResults.Created($"/api/customers/{createdCustomer?.Id}", createdCustomer);
        });
        
        group.MapPut("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, CustomerData customer, ICustomerClient customerClient) =>
        {
            var updated = await customerClient.Update(id, customer);
            return updated
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
        
        group.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, ICustomerClient customerClient) =>
        {
            var deleted = await customerClient.Delete(id);
            return deleted
                ? TypedResults.NoContent()
                : TypedResults.NotFound();
        });
        
        return group;
        
    }
}