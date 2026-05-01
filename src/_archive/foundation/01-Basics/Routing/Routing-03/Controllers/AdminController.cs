using Microsoft.AspNetCore.Mvc;
namespace AspNetCore.AttributeRoute.Controllers
{
    #region 在 Controller上使用路由
    //[Route("News/[controller]/USA/[action]/{id?}")]
    #endregion
    #region 路由约束
    //[Route("[controller]/[action]/{id:int}")]
    #endregion
    #region 自定义约束
    [Route("News/[controller]/USA/[action]/{id:allowedgods?}")]
    #endregion
    public class AdminController : Controller
    {
        //[Route("[controller]/CallMe")]
        public string Index()
        {
            return "'Admin' Controller, 'Index' View";
        }
        public string List()
        {
            return "'Admin' Controller, 'List' View";
        }
        //[Route("[controller]/CallMe/[action]")]
        public string Show()
        {
            return "'Admin' Controller, 'Show' View";
        }
    }
}