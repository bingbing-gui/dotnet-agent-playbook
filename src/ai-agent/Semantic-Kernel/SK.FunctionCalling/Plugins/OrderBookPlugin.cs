using Microsoft.SemanticKernel;
using SK.FunctionCalling.Models;
using System.ComponentModel;

namespace SK.FunctionCalling.Plugins
{
    public class OrderBookPlugin
    {
        private readonly List<Book> _bookMenu;
        private readonly Dictionary<Guid, Cart> _userCarts;
        private readonly List<Cart> _carts;
        private readonly List<BookTag> _bookTags;

        // 模拟的数据
        public OrderBookPlugin()
        {
            // 假设的书籍菜单
            _bookMenu = new List<Book>
            {
                new Book { Id = 1, Name = "自控力", Price = 8.99 },        // The Power of Self-Control -> 自控力
                new Book { Id = 2, Name = "人类简史", Price = 9.99 },       // Sapiens: A Brief History of Humankind -> 人类简史
                new Book { Id = 3, Name = "活着", Price = 10.99 },          // To Live -> 活着
                new Book { Id = 4, Name = "百年孤独", Price = 12.99 },      // One Hundred Years of Solitude -> 百年孤独
                new Book { Id = 5, Name = "围城", Price = 11.99 },          // Fortress Besieged -> 围城
                new Book { Id = 6, Name = "平凡的世界", Price = 10.49 }     // Ordinary World -> 平凡的世界
            };
            // 模拟的书籍标签
            _bookTags = new List<BookTag>
            {
                new BookTag { Id = 1, Name = "励志" },     // Motivational -> 励志
                new BookTag { Id = 2, Name = "历史" },     // History -> 历史
                new BookTag { Id = 3, Name = "小说" },     // Novel -> 小说
                new BookTag { Id = 4, Name = "哲学" },     // Philosophy -> 哲学
                new BookTag { Id = 5, Name = "社会学" },   // Sociology -> 社会学
                new BookTag { Id = 6, Name = "文学" }      // Literature -> 文学
            };

            // 模拟用户购物车
            _userCarts = new Dictionary<Guid, Cart>();
            _carts = new List<Cart>();
        }

        [KernelFunction("get_book_menu")]
        public Task<List<Book>> GetBookMenuAsync()
        {
            return Task.FromResult(_bookMenu);
        }

        [KernelFunction("add_book_to_cart")]
        [Description("添加书籍到用户购物车;返回新添加的书籍并更新购物车")]
        public Task<CartDelta> AddBookToCart(
            [Description("书名，例如《自控力》")] string name,
            [Description("数量")] int quantity = 1)
        {
            // 假设用户只有一个购物车，并使用随机 ID 来生成 cartId
            Guid cartId = Guid.NewGuid();
            var book = _bookMenu.FirstOrDefault(b => b.Name == name);

            if (book == null) return Task.FromResult(new CartDelta { Success = false });


            var cart = new Cart
            {
                Id = cartId,
                Items = new List<CartItem>
                {
                    new CartItem { Book = book, Quantity = quantity }
                },
                TotalPrice = book.Price * quantity
            };

            _userCarts[cartId] = cart;
            _carts.Add(cart);

            return Task.FromResult(new CartDelta { Success = true, Cart = cart });
        }

        [KernelFunction("remove_book_from_cart")]
        public Task<RemoveBookResponse> RemoveBookFromCart(int bookId)
        {
            // 假设我们从用户的购物车移除指定的书籍
            var cart = _carts.FirstOrDefault(c => c.Items.Any(i => i.Book.Id == bookId));
            if (cart == null) return Task.FromResult(new RemoveBookResponse { Success = false });

            var item = cart.Items.FirstOrDefault(i => i.Book.Id == bookId);
            if (item != null)
            {
                cart.Items.Remove(item);
                cart.TotalPrice -= item.Book.Price * item.Quantity;
            }

            return Task.FromResult(new RemoveBookResponse { Success = true });
        }
        [KernelFunction("get_book_from_cart")]
        [Description("返回用户购物车中书籍的详情; 使用此方法，而不是依赖之前的消息，因为购物车可能已经发生了变化.")]
        public Task<Book> GetBookFromCart(int bookId)
        {
            var cartItem = _carts.SelectMany(c => c.Items).FirstOrDefault(i => i.Book.Id == bookId);
            return Task.FromResult(cartItem?.Book);
        }

        [KernelFunction("get_cart")]
        [Description("返回用户当前的购物车，包含总价和购物车中的所有商品.")]
        public Task<List<Cart>> GetCart()
        {

            // 这里假设只返回第一个用户的购物车
            return Task.FromResult(_carts);
        }

        [KernelFunction("checkout")]
        [Description("这个函数主要负责将用户购物车中的商品进行结算.")]
        public Task<CheckoutResponse> Checkout()
        {
            // 模拟结账过程
            double totalAmount = 0;
            _carts.ForEach(cart => totalAmount += cart.TotalPrice);

            if (_carts == null) return Task.FromResult(new CheckoutResponse { Success = false });
            _carts.Clear(); // 清空购物车    
            return Task.FromResult(new CheckoutResponse { Success = true, TotalAmount = totalAmount });
        }
    }
}
