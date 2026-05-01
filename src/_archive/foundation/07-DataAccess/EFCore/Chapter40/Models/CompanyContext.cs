using Microsoft.EntityFrameworkCore;
using System.Runtime.CompilerServices;

namespace EFCoreCodeFirst.Models
{
    public class CompanyContext:DbContext
    {
        public CompanyContext(DbContextOptions<CompanyContext> dbContextOptions)
        :base(dbContextOptions)
        {

        }
        public DbSet<Information> Information { get; set; }
    }
}
