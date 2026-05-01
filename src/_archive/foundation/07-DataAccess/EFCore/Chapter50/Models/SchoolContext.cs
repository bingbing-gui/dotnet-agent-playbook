using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace EFCoreFluentAPIManyToMany.Models
{
    public class SchoolContext : DbContext
    {
        public DbSet<Teacher> Teacher { get; set; }
        public DbSet<Student> Student { get; set; }
        public SchoolContext(DbContextOptions<SchoolContext> options) : base(options)
        { }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Write Fluent API configurations here
            modelBuilder.Entity<Teacher>()
                        .HasMany(t => t.Student)
                        .WithMany(t => t.Teacher)
                        .UsingEntity(j => j.ToTable("TeacherStudent"));
        }
    }
}
