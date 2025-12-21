using BlazorPlayground.API.Infrastructure;
using BlazorPlayground.API.Model;
using BlazorPlayground.Contracts;
using Microsoft.AspNetCore.Http.HttpResults;

namespace BlazorPlayground.API;

public static class CustomerEndpoints
{
    public static WebApplication MapCustomerEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/customers").WithTags("Customers").RequireAuthorization();
        
        group.MapGet("/", async (AppDbContext db, int page = 1, int pageSize = 10) =>
        {
            var customers = await db.Customers
                .OrderBy(c => c.Id)
                .Select(c=>c.ToResource())
                .ToPagedAsync(page, pageSize);
            
            return TypedResults.Ok(customers);
        });

        group.MapGet("/{id:guid}", async Task<Results<Ok<CustomerResource>, NotFound>> (Guid id, AppDbContext db) =>
        {
            var customer = await db.Customers.FindAsync(id);

            return customer is null
                ? TypedResults.NotFound()
                : TypedResults.Ok(customer.ToResource());
        });

        group.MapPost("/", async Task<Created<CustomerResource>> (CustomerData customer, AppDbContext db) =>
        {
            var newCustomer = new BlazorPlayground.API.Model.Customer
            {
                CompanyName = customer.CompanyName,
                ContactName = customer.ContactName,
                Address = customer.Address,
                City = customer.City,
                Region = customer.Region,
                PostalCode = customer.PostalCode,
                Country = customer.Country,
                Phone = customer.Phone
            };
            
            db.Customers.Add(newCustomer);
            
            await db.SaveChangesAsync();
            
            var resource = newCustomer.ToResource();

            return TypedResults.Created($"/api/customers/{newCustomer.Id}", resource);
        });
        
        group.MapPut("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, CustomerData customer, AppDbContext db) =>
        {
            if (await db.Customers.FindAsync(id) is not { } existing)
            {
                return TypedResults.NotFound();
            }
            
            existing.ReplaceWith(customer);

            await db.SaveChangesAsync();
            
            return TypedResults.NoContent();
        });
        
        group.MapDelete("/{id:guid}", async Task<Results<NoContent, NotFound>> (Guid id, AppDbContext db) =>
        {
            if (await db.Customers.FindAsync(id) is not { } existing)
            {
                return TypedResults.NotFound();
            }
            
            db.Customers.Remove(existing);
            
            await db.SaveChangesAsync();
            
            return TypedResults.NoContent();
        });

        return app;
    }
}