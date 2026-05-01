using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EFCoreFluentAPIOneToOne.Models
{
    public class CountryContext : DbContext
    {
        public DbSet<City> City { get; set; }
        public DbSet<CityInformation> CityInformation { get; set; }
        public CountryContext(DbContextOptions<CountryContext> options) : base(options)
        {
        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
        //Write Fluent API configurations here​
            modelBuilder.Entity<City>()
                        .HasOne(e => e.CityInformation)
                        .WithOne(e => e.City)
                        .HasForeignKey<City>(e => e.CityInformationId);
        }
    }
}
