using System.Net;
using System.Net.Http.Json;
using BlazorPlayground.Contracts;

namespace BlazorPlayground.FunctionalTests;

public class CustomerApiTests : IClassFixture<ApiFactory>
{
    private readonly HttpClient _client;

    public CustomerApiTests(ApiFactory fixture)
    {
        _client = fixture.CreateClient();
    }

    [Fact]
    public async Task Get_Customers_Returns_Ok_And_PagedResult()
    {
        // Act
        var response = await _client.GetAsync("/api/customers");

        // Assert
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<PagedResult<CustomerResource>>();
        Assert.NotNull(result);
        Assert.True(result!.PageIndex >= 1);
        Assert.True(result.PageSize >= 1);
        Assert.NotNull(result.Items);
    }

    [Fact]
    public async Task Get_Customer_UnknownId_Returns_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"/api/customers/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Post_Customer_Then_Get_ById_Roundtrips()
    {
        // Arrange
        var newCustomer = new CustomerData
        {
            CompanyName = "Test Company",
            ContactName = "Test Contact",
            Address = "123 Test St",
            City = "Testville",
            Region = "TS",
            PostalCode = "12345",
            Country = "Testland",
            Phone = "123-456-7890"
        };

        // Act – create
        var createResponse = await _client.PostAsJsonAsync("/api/customers", newCustomer);

        // Assert create
        createResponse.EnsureSuccessStatusCode();
        var created = await createResponse.Content.ReadFromJsonAsync<CustomerResource>();
        Assert.NotNull(created);
        Assert.NotEqual(Guid.Empty, created!.Id);
        Assert.Equal(newCustomer.CompanyName, created.CompanyName);

        // Act – get by id
        var getResponse = await _client.GetAsync($"/api/customers/{created.Id}");

        // Assert get
        getResponse.EnsureSuccessStatusCode();
        var fetched = await getResponse.Content.ReadFromJsonAsync<CustomerResource>();
        Assert.NotNull(fetched);
        Assert.Equal(created.Id, fetched!.Id);
        Assert.Equal(created.CompanyName, fetched.CompanyName);
    }

    [Fact]
    public async Task Delete_UnknownCustomer_Returns_NotFound()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"/api/customers/{id}");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}