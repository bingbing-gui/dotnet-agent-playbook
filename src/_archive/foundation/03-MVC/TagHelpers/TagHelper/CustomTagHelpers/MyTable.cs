using AspNetCore.TagHelpers.Models;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Text;
namespace AspNetCore.TagHelpers.CustomTagHelpers
{
    [HtmlTargetElement("MyTable")]
    public class MyTable : TagHelper
    {
        public IEnumerable<Product> Products { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "table";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class","table table-sm table-bordered");
            StringBuilder sb = new StringBuilder();
            foreach (var product in Products)
                sb.Append("<tr><td>" + @product.Name + "</td><td>" + @product.Price
                + "</td><td>" + @product.Quantity + "</td></tr>");
            output.Content.SetHtmlContent($@"<thead class=""bg-dark text-white"">
                                            <tr>
                                                <th>Name</th>
                                                <th>Price</th>
                                                <th>Quantity</th>
                                            </tr>
                                        </thead>
                                        {sb.ToString()}<tbody></tbody>");
        }
    }
}
