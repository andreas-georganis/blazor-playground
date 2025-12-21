using BlazorPlayground.Contracts;
using BlazorPlayground.WebApp.Client.Services;

namespace BlazorPlayground.WebApp.Services;

public class CustomerClient(HttpClient http) : ICustomerClient
{
    public async Task<PagedResult<CustomerResource>> Get(int pageNumber, int pageSize)
    {
        return await http.GetFromJsonAsync<PagedResult<CustomerResource>>("api/customers" +$"?page={pageNumber}&pageSize={pageSize}") 
               ?? new PagedResult<CustomerResource>([], 0, 0, 0);
    }
    
    public async Task<CustomerResource?> Get(Guid id)
    {
        return await http.GetFromJsonAsync<CustomerResource>($"api/customers/{id}");
    }
    
    public async Task<CustomerResource?> Create(CustomerData customer)
    {
        var response = await http.PostAsJsonAsync("api/customers", customer);
        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        return await response.Content.ReadFromJsonAsync<CustomerResource>();
    }
    
    public async Task<bool> Update(Guid id, CustomerData customer)
    {
        var response = await http.PutAsJsonAsync($"api/customers/{id}", customer);
        return response.IsSuccessStatusCode;
    }
    
    public async Task<bool> Delete(Guid id)
    {
        var response = await http.DeleteAsync($"api/customers/{id}");
        return response.IsSuccessStatusCode;
    }
}