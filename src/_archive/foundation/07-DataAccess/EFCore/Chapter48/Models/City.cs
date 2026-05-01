namespace EFCoreFluentAPIOneToOne.Models
{
    public class City
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CityInformationId { get; set; }
        public CityInformation CityInformation { get; set; }
    }
}
