using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace UnisaveCompiler.Http
{
    public class JsonRoute<TRequest, TResponse> : Route
    {
        private readonly Func<TRequest, HttpListenerRequest, Task<TResponse>> handler;
        
        public JsonRoute(
            HttpMethod method,
            string url,
            Func<TRequest, HttpListenerRequest, Task<TResponse>> handler,
            string token
        ) : base(method, url, token)
        {
            this.handler = handler
                ?? throw new ArgumentNullException(nameof(handler));
        }
        
        public override async Task<HttpResponse> InvokeAsync(
            HttpListenerRequest request
        )
        {
            var requestBody = JsonConvert.DeserializeObject<TRequest>(
                await new StreamReader(request.InputStream).ReadToEndAsync()
            );

            TResponse responseBody = await handler.Invoke(requestBody, request);

            return new JsonResponse<TResponse>(responseBody);
        }
    }
    
    public class JsonRoute<TResponse> : Route
    {
        private readonly Func<HttpListenerRequest, Task<TResponse>> handler;
        
        public JsonRoute(
            HttpMethod method,
            string url,
            Func<HttpListenerRequest, Task<TResponse>> handler,
            string token
        ) : base(method, url, token)
        {
            this.handler = handler
                           ?? throw new ArgumentNullException(nameof(handler));
        }
        
        public override async Task<HttpResponse> InvokeAsync(
            HttpListenerRequest request
        )
        {
            TResponse responseBody = await handler.Invoke(request);

            return new JsonResponse<TResponse>(responseBody);
        }
    }
}