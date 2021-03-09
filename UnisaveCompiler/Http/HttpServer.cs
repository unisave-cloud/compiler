using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace UnisaveCompiler.Http
{
    public class HttpServer : IDisposable
    {
        private readonly HttpListener listener;
        private readonly Router router;

        private Task listeningLoopTask;

        private bool alreadyDisposed;
        
        public HttpServer(int port, Router router)
        {
            listener = new HttpListener();
            listener.Prefixes.Add("http://*:" + port + "/");
            
            // we will accept basic http auth for some routes
            listener.AuthenticationSchemeSelectorDelegate += request => {
                var route = router.ResolveRoute(request);
                return route.SecretToken == null ?
                    AuthenticationSchemes.None :
                    AuthenticationSchemes.Basic;
            };

            this.router = router;
        }
 
        public void Start()
        {
            if (alreadyDisposed)
                throw new ObjectDisposedException(
                    "Http server has been already stopped."
                );
            
            listener.Start();

            // start listening in a new task
            listeningLoopTask = ListeningLoopAsync();
        }

        private async Task ListeningLoopAsync()
        {
            // WARNING: access only from within this method
            // to make sure no race condition occurs
            List<Task> pendingRequests = new List<Task>();
            
            while (listener.IsListening)
            {
                HttpListenerContext ctx;
                
                // wait for a request
                try
                {
                    ctx = await listener.GetContextAsync();
                }
                catch (ObjectDisposedException)
                {
                    // listener has been stopped, break the loop
                    break;
                }
                
                pendingRequests.Add(
                    HandleConnectionAsync(ctx)
                );
                
                // clean up finished tasks
                pendingRequests.RemoveAll(t => {
                    if (!t.IsCompleted)
                        return false;

                    // process any exceptions before removing the task
                    t.Wait();
                    return true;
                });
            }
            
            // wait for all pending requests to finish
            await Task.WhenAll(pendingRequests);
        }

        private async Task HandleConnectionAsync(HttpListenerContext context)
        {
            // NOTE: this method should not really throw an exception
            // if it does, it will take down the entire server
            
            if (context == null)
                throw new ArgumentNullException(nameof(context));

            // make sure we *will* run asynchronously
            await Task.Yield();

            try
            {
                Route route = router.ResolveRoute(context.Request);

                // authenticate
                if (route.SecretToken != null)
                {
                    var identity = (HttpListenerBasicIdentity) context.User.Identity;
                    
                    if (!route.Authenticate(identity))
                    {
                        // authentication failed
                        new HttpResponse { StatusCode = 401 }
                            .Send(context.Response);
                        
                        return;
                    }
                }
                
                // invoke
                HttpResponse response = await route.InvokeAsync(context.Request);
                
                response.Send(context.Response);
            }
            catch (Exception e)
            {
                Log.Error(
                    "An exception occured when " +
                    "processing an HTTP request:\n" + e
                );
                
                var exceptionResponse = new ExceptionResponse(e);
                exceptionResponse.Send(context.Response);
            }
            finally
            {
                context.Response.OutputStream.Close();
            }
        }
 
        public void Stop()
        {
            if (alreadyDisposed)
                return;
            
            listener.Stop();
            listener.Close();

            listeningLoopTask?.Wait();
            listeningLoopTask = null;

            alreadyDisposed = true;
        }

        public void Dispose()
        {
            Stop();
        }
    }
}