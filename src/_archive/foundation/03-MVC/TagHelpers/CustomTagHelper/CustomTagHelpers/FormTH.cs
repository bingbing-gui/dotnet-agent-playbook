using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace AspNetCore.CustomTagHelpers.CustomTagHelpers
{
    [HtmlTargetElement("form")]
    public class FormTH: TagHelper
    {
        private IUrlHelperFactory urlHelperFactory;
        public FormTH(IUrlHelperFactory factory)
        {
            urlHelperFactory = factory;
        }

        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContextData { get; set; }

        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            IUrlHelper urlHelper = urlHelperFactory.GetUrlHelper(ViewContextData);
            output.Attributes.SetAttribute("action", 
                urlHelper.Action(ViewContextData.RouteData.Values["action"].ToString(), 
                ViewContextData.RouteData.Values["controller"].ToString()));
        }
    }
}
