using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace AspNetCore.CustomTagHelpers.Models
{
    public class Product
    {
        [DisplayName("名称")]
        public string Name { get; set; }
        [DisplayName("价格")]
        public float Price { get; set; }
        [DisplayName("数量")]
        public int Quantity { get; set; }
    }
}
