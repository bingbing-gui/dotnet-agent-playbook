namespace AspNetCore.APIControllers.Models
{
    public class Reservation
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string StartLocation { get; set; }
        public string EndLocation { get; set; }
    }
}
