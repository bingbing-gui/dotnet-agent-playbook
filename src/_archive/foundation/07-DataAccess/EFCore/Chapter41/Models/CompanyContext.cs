using Microsoft.EntityFrameworkCore;

namespace EFCoreMigration.Models
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> options)
                      : base(options)
        {
        }
        public DbSet<Client> Clients { get; set; }
       
    }
}
