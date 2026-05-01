namespace EFCoreInsertRecords.Models
{
    public class Department
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<Employee> Employee { get; set; }
    }
}
