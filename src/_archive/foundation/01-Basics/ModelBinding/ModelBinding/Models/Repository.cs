namespace AspNetCore.ModelBinding.Models
{
    public interface IRepository
    {
        IEnumerable<Employee> Employees { get; }
        Employee this[int id] { get; set; }
    }
    public class EmployeeRepository : IRepository
    {
        private Dictionary<int, Employee> employee = new Dictionary<int, Employee>
        {
            [1] = new Employee
            {
                Id = 1,
                Name = "John",
                DOB = new DateTime(1980, 12, 25),
                Role = Role.Admin
            },
            [2] = new Employee
            {
                Id = 2,
                Name = "Michael",
                DOB = new DateTime(1981, 5, 13),
                Role = Role.Designer
            },
            [3] = new Employee
            {
                Id = 3,
                Name = "Rachael",
                DOB = new DateTime(1982, 11, 25),
                Role = Role.Designer
            },
            [4] = new Employee
            {
                Id = 4,
                Name = "Anna",
                DOB = new DateTime(1983, 1, 20),
                Role = Role.Manager
            }
        };
        public IEnumerable<Employee> Employees=> employee.Values;
        public Employee this[int id]
        {
            get
            {
                return employee.ContainsKey(id) ? employee[id] : null;
            }
            set
            {
                employee[id] = value;
            }
        }
    }
}
