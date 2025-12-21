using BlazorPlayground.Contracts;

namespace BlazorPlayground.API.Model;

public class Customer
{
    public Guid Id { get; set; }
    public string? CompanyName { get; set; }
    public string? ContactName { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Region { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
    public string? Phone { get; set; }
        
    public void ReplaceWith(CustomerData other)
    {
        ArgumentNullException.ThrowIfNull(other);
            
        CompanyName = other.CompanyName;
        ContactName = other.ContactName;
        Address = other.Address;
        City = other.City;
        Region = other.Region;
        PostalCode = other.PostalCode;
        Country = other.Country;
        Phone = other.Phone;
    }
}

public static class CustomerExtensions
{
    public static CustomerResource ToResource(this Customer customer)
    {
        ArgumentNullException.ThrowIfNull(customer);
            
        return new CustomerResource
        {
            Id = customer.Id,
            CompanyName = customer.CompanyName.IsEmptyToNull(),
            ContactName = customer.ContactName.IsEmptyToNull(),
            Address = customer.Address.IsEmptyToNull(),
            City = customer.City.IsEmptyToNull(),
            Region = customer.Region.IsEmptyToNull(),
            PostalCode = customer.PostalCode.IsEmptyToNull(),
            Country = customer.Country.IsEmptyToNull(),
            Phone = customer.Phone.IsEmptyToNull()
        };
    }
}

public static class StringExtensions
{
    extension(string? value)
    {
        public string? IsEmptyToNull()
        {
            return string.IsNullOrWhiteSpace(value) ? null : value;
        }
    }
}

// This could be useful for future refactoring to a task-based api
record Company(string Name);
record Address(string Street, string City, string Region, string PostalCode, string Country);
record Contact(string Name, string Phone);