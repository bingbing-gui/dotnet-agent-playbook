using AspNetCore.HttpClientWithHttpVerb.Models;
using AspNetCore.UsingHttpVerb.Practice.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace AspNetCore.HttpClientWithHttpVerb.Handlers
{
    public class OperationHandler: DelegatingHandler
    {
        private readonly IOperationScoped _operationScoped;

        public OperationHandler(IOperationScoped operationScoped)
        {
            _operationScoped = operationScoped;
        }
        protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Add("X-OPERATION-ID", _operationScoped.OperationId);

            return await base.SendAsync(request, cancellationToken);
        }
    }
}
