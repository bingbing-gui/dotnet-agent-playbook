namespace EFCoreFluentAPIManyToMany.Models
{
    public class Student
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public IList<Teacher> Teacher { get; set; } //collection navigation property
    }
}
