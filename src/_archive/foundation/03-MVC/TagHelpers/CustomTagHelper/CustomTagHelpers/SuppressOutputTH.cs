using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCore.CustomTagHelpers.CustomTagHelpers
{
    [HtmlTargetElement(Attributes = "action-name")]
    public class SuppressOutputTH:TagHelper
    {
        public string ActionName { get; set; }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (!ViewContext.RouteData.Values["action"].ToString().Equals(ActionName))
                output.SuppressOutput();
        }
    }
}
