using Microsoft.AspNetCore.Mvc.Filters;
using System.Text;

namespace AspNetCore.Filters.CustomFilters
{
    public class ShowMessage : Attribute, IResultFilter, IOrderedFilter
    {
        private string message;
        public ShowMessage(string msg)
        {
            message = msg;
        }
        public int Order { get; set; }
        public void OnResultExecuted(ResultExecutedContext context)
        {

        }
        public void OnResultExecuting(ResultExecutingContext context)
        {
            WriteMessage(context, message);
        }
        private void WriteMessage(FilterContext context, string msg)
        {
            byte[] bytes = Encoding.ASCII.GetBytes($"<div>{msg}</div>");
            context.HttpContext.Response.Body.WriteAsync(bytes, 0, bytes.Length);
        }
    }
}
