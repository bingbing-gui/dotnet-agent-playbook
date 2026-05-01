using AspNetCore.Views.Models;
using AspNetCore.Views.Service;
using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.Views.Components
{
    public class Cart : ViewComponent
    {
        private Coupon _coupon;
        public Cart(Coupon coupon)
        {
            _coupon = coupon;
        }
        #region
        //public string Invoke()
        //{
        //    return "This is from View Component";
        //}
        //public IViewComponentResult Invoke()
        //{
        //    return Content("This is from <h2>View Component</h2>");
        //}
        //    public IViewComponentResult Invoke()
        //    {
        //        return new HtmlContentViewComponentResult(new HtmlString("This is from <h2>View Component</h2>"));
        //    }
        #endregion
        #region 返回复杂视图
        public IViewComponentResult Invoke(bool showCart)
        {
            Product[] products;
            if (showCart)
            {
                products = new Product[] {
                new Product() { Name = "Women Shoes", Price = 99 },
                new Product() { Name = "Mens Shirts", Price = 59 },
                new Product() { Name = "Children Belts", Price = 19 },
                new Product() { Name = "Girls Socks", Price = 9 }
                };

             ViewBag.Coupon = _coupon.GetCoupon();
            }
            else
            {
                products = new Product[] { };
            }
            return View(products);
        }
        #endregion
    }
}
