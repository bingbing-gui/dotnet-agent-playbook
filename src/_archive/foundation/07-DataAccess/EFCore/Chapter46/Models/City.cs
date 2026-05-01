namespace EFCoreConventions.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public ICollection<Country> Country { get; set; } //Collection Navigation Property
    }
}
