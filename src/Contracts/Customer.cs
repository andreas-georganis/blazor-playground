namespace BlazorPlayground.Contracts
{
    public class CustomerData
    {
        public string? CompanyName { get; set; }
        public string? ContactName { get; set; }
        public string? Address { get; set; }
        public string? City { get; set; }
        public string? Region { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
        public string? Phone { get; set; }
    }
    
    public class CustomerResource : CustomerData
    {
        public Guid Id { get; set; }
        
    }
}
