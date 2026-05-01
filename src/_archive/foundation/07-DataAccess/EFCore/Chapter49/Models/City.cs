namespace EFCoreFluentAPIOneToMany.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int FKCountry { get; set; }
        public Country Country { get; set; }
    }
}
