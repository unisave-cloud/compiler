using System;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace UnisaveCompiler.Http
{
    /// <summary>
    /// Passes a given HTTP request to an appropriate handler method
    /// </summary>
    public class Router
    {
        private readonly string secretToken;
        
        // routes
        private readonly Func<HttpListenerContext, Task<string>> indexPage;
        private readonly Func<CompilationRequest, Task<CompilationResponse>>
            compileBackend;
        
        public Router(
            string secretToken,
            Func<HttpListenerContext, Task<string>> indexPage,
            Func<CompilationRequest, Task<CompilationResponse>> compileBackend
        )
        {
            this.secretToken = secretToken;
            
            this.indexPage = indexPage;
            this.compileBackend = compileBackend;
        }
        
        /// <summary>
        /// Entrypoint into the router
        /// </summary>
        /// <param name="context">The request context</param>
        /// <exception cref="ArgumentNullException"></exception>
        public Task HandleRequestAsync(HttpListenerContext context)
        {
            if (context == null)
                throw new ArgumentNullException(nameof(context));
            
            // authenticate
            var identity = (HttpListenerBasicIdentity)context.User.Identity;
            if (identity.Name != "api")
                return UnauthorizedResponse(context, "Invalid username.");
            if (identity.Password != secretToken)
                return UnauthorizedResponse(
                    context,
                    "Invalid secret token (password)."
                );

            // handle request
            switch (context.Request.HttpMethod)
            {
                case "GET":
                    return GetRouteAsync(context);
                
                case "POST":
                    return PostRouteAsync(context);
                
                default:
                    DefaultRoute(context);
                    return Task.CompletedTask;
            }
        }
        
        private Task UnauthorizedResponse(
            HttpListenerContext context,
            string message
        )
        {
            Log.Warning("Failed authentication attempt");
            
            context.Response.StatusCode = 401;
            StringResponse(context, "401 - Unauthorized.\n\n" + message);
            return Task.CompletedTask;
        }

        private async Task GetRouteAsync(HttpListenerContext context)
        {
            if (context.Request.Url.AbsolutePath == "/")
            {
                StringResponse(context, await indexPage.Invoke(context));
                return;
            }
            
            DefaultRoute(context);
        }
        
        private async Task PostRouteAsync(HttpListenerContext context)
        {
            if (context.Request.Url.AbsolutePath == "/compile-backend")
            {
                var request = await JsonSerializer
                    .DeserializeAsync<CompilationRequest>(
                        context.Request.InputStream
                    );
                CompilationResponse response = await compileBackend.Invoke(
                    request
                );
                string responseString = JsonSerializer.Serialize(response);
                
                StringResponse(
                    context,
                    responseString,
                    "application/json"
                );
                return;
            }
            
            DefaultRoute(context);
        }

        private void DefaultRoute(HttpListenerContext context)
        {
            context.Response.StatusCode = 404;
            StringResponse(context, "404 - Page not found.\n");
        }

        /// <summary>
        /// Sends a string response encoded into UTF-8
        /// </summary>
        /// <param name="context">Request context</param>
        /// <param name="response">The actual string to send</param>
        /// <param name="contentType">What content type to present it as</param>
        private void StringResponse(
            HttpListenerContext context,
            string response,
            string contentType = "text/plain"
        )
        {
            if (response == null)
                response = "null";
            
            try
            {
                byte[] responseBytes = Encoding.UTF8.GetBytes(response);
                context.Response.Headers.Add("Content-Type", contentType);
                context.Response.ContentLength64 = responseBytes.Length;
                context.Response.OutputStream.Write(
                    responseBytes,
                    0,
                    responseBytes.Length
                );
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
    }
}