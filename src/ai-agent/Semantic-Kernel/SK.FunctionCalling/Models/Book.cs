namespace SK.FunctionCalling.Models
{
    public class Book
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }
    }

    public class Cart
    {
        public Guid Id { get; set; }
        public List<CartItem> Items { get; set; }
        public double TotalPrice { get; set; }
    }

    public class CartItem
    {
        public Book Book { get; set; }
        public int Quantity { get; set; }
        public string SpecialInstructions { get; set; }
        public List<BookTag> Tags { get; set; }
    }

    public class BookTag
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class CartDelta
    {
        public bool Success { get; set; }
        public Cart Cart { get; set; }
    }

    public class RemoveBookResponse
    {
        public bool Success { get; set; }
    }

    public class CheckoutResponse
    {
        public bool Success { get; set; }
        public double TotalAmount { get; set; }
    }
}
