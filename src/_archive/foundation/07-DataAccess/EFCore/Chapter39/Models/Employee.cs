namespace EFCoreDbContext.Models
{
    public class Employee
    {
        public int Id { get; set; }
        public int DepartmentId { get; set; }
        public string Name { get; set; }
        public string Designation { get; set; }
        public Department Department { get; set; }
    }
}
