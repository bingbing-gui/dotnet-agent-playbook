namespace SK.VectorStores.ViewModes
{
    public class HotelSearchVm
    {
        public string? Query { get; set; }
        public int TopK { get; set; } = 5;
        public List<HotelSearchResultVm> Results { get; set; } = new();
    }
}
