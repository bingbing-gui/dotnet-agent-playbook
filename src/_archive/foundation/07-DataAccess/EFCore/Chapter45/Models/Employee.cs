using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace EFCoreDeleteRecords.Models
{
    public class Employee
    {
        [DisplayName("编号")]
        public int Id { get; set; }
        [DisplayName("部门")]
        public int DepartmentId { get; set; }
        [DisplayName("姓名")]
        public string Name { get; set; }
        [DisplayName("职务")]
        public string Designation { get; set; }
        [DisplayName("部门")]
        public Department Department { get; set; }
    }
}
