using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCore.CustomTagHelpers.CustomTagHelpers
{
    [HtmlTargetElement("div", Attributes = "pre-post")]
    public class PrePostElementTH : TagHelper
    {
        public bool AddHeader { get; set; } = true;
        public bool AddFooter { get; set; } = true;
        public string PrePost { get; set; }
        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            output.Attributes.SetAttribute("class", "m-1 p-1");

            TagBuilder title = new TagBuilder("h1");
            title.InnerHtml.Append(PrePost);
            TagBuilder container = new TagBuilder("div");
            container.Attributes["class"] = "bg-info m-1 p-1";
            container.InnerHtml.AppendHtml(title);

            if (AddHeader)
                output.PreElement.SetHtmlContent(container);
            if (AddFooter)
                output.PostElement.SetHtmlContent(container);
        }
    }
}
