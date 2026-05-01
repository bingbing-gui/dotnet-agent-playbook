
using Microsoft.EntityFrameworkCore;

namespace EFCoreDeleteRecords.Models
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> dbContextOptions)
            : base(dbContextOptions)
        {

        }
        public DbSet<Department> Department { get; set; }
        public DbSet<Employee> Employee { get; set; }
    }
}
