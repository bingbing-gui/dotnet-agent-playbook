using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace AspNetCore.Filters.CustomFilters
{

public interface IExceptionFilterMessage
{
    IEnumerable<string> Messages { get; }
    void AddMessage(string message);
}

public class ExceptionFilterMessage : IExceptionFilterMessage
{
    private List<string> messages = new List<string>();
    public IEnumerable<string> Messages => messages;
    public void AddMessage(string message) => messages.Add(message);
}
public class CatchErrorMessage : IExceptionFilter
{
    private IExceptionFilterMessage _exceptionFilterMessage;

    public CatchErrorMessage(IExceptionFilterMessage exceptionFilterMessage)
    {
        _exceptionFilterMessage = exceptionFilterMessage;
    }
    public void OnException(ExceptionContext context)
    {
        _exceptionFilterMessage.AddMessage("Exception Filter is called. ");
        _exceptionFilterMessage.AddMessage("Error Message is given below. ");
        _exceptionFilterMessage.AddMessage(context.Exception.Message);

        string allMessage = "";
        foreach (string message in _exceptionFilterMessage.Messages)
        {
            allMessage += message;
        }
        context.Result = new ViewResult()
        {
            ViewData = new ViewDataDictionary(
                new EmptyModelMetadataProvider(),
                new ModelStateDictionary())
            {
                Model = allMessage
            }
        };
    }
}

}
