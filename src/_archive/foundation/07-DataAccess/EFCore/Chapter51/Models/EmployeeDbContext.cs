using Microsoft.EntityFrameworkCore;

namespace EFCoreExecuteRawSql.Models
{
    public class EmployeeDbContext : DbContext
    {
        public EmployeeDbContext(DbContextOptions<EmployeeDbContext> dbContextOptions) : base(dbContextOptions)
        {

        }
        public DbSet<Employee> Employees { get; set; }

        public DbSet<Project> Projects { get; set; }
    }
}
