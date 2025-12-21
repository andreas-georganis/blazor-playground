using System.Net.Http.Json;
using BlazorPlayground.Contracts;

namespace BlazorPlayground.WebApp.Client.Services;

public interface ICustomerClient
{
    Task<PagedResult<CustomerResource>> Get(int pageNumber, int pageSize);
    Task<CustomerResource?> Get(Guid id);
    Task<CustomerResource?> Create(CustomerData customer);
    Task<bool> Update(Guid id, CustomerData customer);
    Task<bool> Delete(Guid id);
}
        

public class CustomerClient(HttpClient http) : ICustomerClient
{
    private const string BaseUrl = "webapp/customers";
    
    public async Task<PagedResult<CustomerResource>> Get(int pageNumber, int pageSize)
    {
        return await http.GetFromJsonAsync<PagedResult<CustomerResource>>($"{BaseUrl}" +$"?page={pageNumber}&pageSize={pageSize}") 
            ?? new PagedResult<CustomerResource>([], 0, 0, 0);
    }
    
    public async Task<CustomerResource?> Get(Guid id)
    {
        return await http.GetFromJsonAsync<CustomerResource>($"{BaseUrl}/{id}");
    }
    
    public async Task<CustomerResource?> Create(CustomerData customer)
    {
        var response = await http.PostAsJsonAsync($"{BaseUrl}", customer);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<CustomerResource>();
    }
    
    public async Task<bool> Update(Guid id, CustomerData customer)
    {
        var response = await http.PutAsJsonAsync($"{BaseUrl}/{id}", customer);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> Delete(Guid id)
    {
        var response = await http.DeleteAsync($"{BaseUrl}/{id}");
        return response.IsSuccessStatusCode;
    }
}