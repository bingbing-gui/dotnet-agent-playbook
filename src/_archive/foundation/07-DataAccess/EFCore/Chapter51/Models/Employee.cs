namespace EFCoreExecuteRawSql.Models
{
public class Employee
{
    public int Id { get; set; }
    public string Department { get; set; }
    public string Name { get; set; }
    public string Designation { get; set; }
    public Project Project { get; set; }
}
}
