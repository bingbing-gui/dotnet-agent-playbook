namespace AspNetCore.RouteConstraint.CustomConstraint
{
    public class OnlyGodsConstraint : IRouteConstraint
    {
        private string[] gods = new[] { "Ram", "Shiv", "Krishn", "Vishnu", "Brahma","Lakshmi" };
        public bool Match(HttpContext? httpContext, IRouter? route, 
                          string routeKey, RouteValueDictionary values, 
                          RouteDirection routeDirection)
        {
            return gods.Contains(values[routeKey]);
        }
    }
}
