namespace AspNetCore.ModelBinding.Advanced.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime DOB { get; set; }
        public Address HomeAddress { get; set; }
        public Role Role { get; set; }
    }
    public class Address
    {
        public string HouseNumber { get; set; }
        public string Street { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
    public enum Role
    {
        Admin,
        Designer,
        Manager
    }
}
