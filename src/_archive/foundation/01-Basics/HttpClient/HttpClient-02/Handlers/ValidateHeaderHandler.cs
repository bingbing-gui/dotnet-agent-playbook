using System.Net.Http;
using System.Net;
using System.Threading.Tasks;
using System.Threading;

namespace AspNetCore.HttpClientWithHttpVerb.Handlers;

public class ValidateHeaderHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (!request.Headers.Contains("X-API-KEY"))
        {
            return new HttpResponseMessage(HttpStatusCode.BadRequest)
            {
                Content = new StringContent(
                    "The API key header X-API-KEY is required.")
            };
        }
        return await base.SendAsync(request, cancellationToken);
    }
}

