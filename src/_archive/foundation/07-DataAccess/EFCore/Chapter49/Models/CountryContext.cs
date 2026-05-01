using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EFCoreFluentAPIOneToMany.Models
{
    public class CountryContext: DbContext
    {
        public DbSet<City> City { get; set; }
        public DbSet<Country> Country { get; set; }
        public CountryContext(DbContextOptions<CountryContext> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Write Fluent API configurations here
            modelBuilder.Entity<City>()
                        .HasOne(e => e.Country)
                        .WithMany(e => e.City)
                        .HasForeignKey(e => e.FKCountry)
                        .OnDelete(DeleteBehavior.Cascade); ;
        }
    }
}
