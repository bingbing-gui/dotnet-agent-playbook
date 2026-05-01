using Microsoft.EntityFrameworkCore;

namespace EFCoreConfiguration.Models
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> contextOptions)
            : base(contextOptions)
        {

        }
        public DbSet<City> Cities { get; set; }

        public DbSet<Country> Countries { get; set; }
    }
}
