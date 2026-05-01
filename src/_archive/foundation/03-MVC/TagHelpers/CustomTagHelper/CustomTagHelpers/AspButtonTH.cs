using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCore.CustomTagHelpers.CustomTagHelpers
{
    [HtmlTargetElement("aspbutton")]
    public class AspButtonTH:TagHelper
    {
        public string Type { get; set; } = "Submit";
        public string BackgroundColor { get; set; } = "primary";
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = "button";
            output.TagMode = TagMode.StartTagAndEndTag;
            output.Attributes.SetAttribute("class", $"btn btn-{BackgroundColor}");
            output.Attributes.SetAttribute("type", Type);
            output.Content.SetContent("Click to Add Record");
        }
    }
}
