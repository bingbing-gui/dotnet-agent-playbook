namespace EFCoreExecuteRawSql.Models
{
public class Project
{
    public int Id { get; set; }
    public string Name { get; set; }
    public ICollection<Employee> Employee { get; set; }
}
}
