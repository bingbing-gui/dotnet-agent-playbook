using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace EFCoreConventions.Models
{
    public class CompanyContext : DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> options) : base(options)
        {

        }

        public DbSet<Employee> Employee { get; set; }

        public DbSet<Country> Country { get; set; }
    }
}
