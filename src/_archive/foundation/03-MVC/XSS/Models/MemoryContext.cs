using Microsoft.EntityFrameworkCore;

namespace AspNetCore.XSS.Models
{
    public class MemoryContext : DbContext
    {
        public MemoryContext(DbContextOptions<MemoryContext> dbContextOptions)
           : base(dbContextOptions)
        {

        }
        public DbSet<Blog> Blogs { get; set; }
    }
}
