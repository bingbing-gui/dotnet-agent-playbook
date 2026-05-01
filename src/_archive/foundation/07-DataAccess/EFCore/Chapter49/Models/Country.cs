namespace EFCoreFluentAPIOneToMany.Models
{
    public class Country
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ICollection<City> City { get; set; }
    }
}
