using System.ComponentModel;

namespace EFCoreDeleteRecords.Models
{
    public class Department
    {
        [DisplayName("编号")]
        public int Id { get; set; }
        [DisplayName("姓名")]
        public string Name { get; set; }
        public ICollection<Employee> Employee { get; set; }
    }
}
