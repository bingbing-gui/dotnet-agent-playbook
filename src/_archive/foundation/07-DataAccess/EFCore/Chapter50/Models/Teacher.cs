namespace EFCoreFluentAPIManyToMany.Models
{
    public class Teacher
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Student> Student { get; set; } //collection navigation property
    }
}
