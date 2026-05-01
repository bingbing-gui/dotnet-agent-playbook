using Microsoft.AspNetCore.Razor.TagHelpers;
namespace AspNetCore.CustomTagHelpers.CustomTagHelpers
{
    [HtmlTargetElement(Attributes = "background-color")]
    public class BackgroundColorTH : TagHelper
    {
        public string BackgroundColor { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("class", $"btn btn-{BackgroundColor}");
        }
    }
}
