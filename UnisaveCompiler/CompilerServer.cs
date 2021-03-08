using System;
using System.Diagnostics;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using DotNetEnv;
using UnisaveCompiler.Http;

namespace UnisaveCompiler
{
    /// <summary>
    /// Script runner server that accepts the HTTP requests
    /// </summary>
    public class CompilerServer : IDisposable
    {
        private readonly HttpServer httpServer;

        public CompilerServer()
        {
            httpServer = new HttpServer(
                Env.GetInt("LISTENING_PORT"),
                new Router(
                    secretToken: Env.GetString("SECRET_TOKEN"),
                    indexPage: IndexPage,
                    compileBackend: CompileBackend
                )
            );
        }
        
        /// <summary>
        /// Start the compiler server
        /// </summary>
        public void Start()
        {
            PrintStartupMessage();
            
            httpServer.Start();
            
            Log.Info("Server running.");
        }

        private void PrintStartupMessage()
        {
            string version = typeof(CompilerServer).Assembly
                .GetCustomAttribute<AssemblyInformationalVersionAttribute>()
                .InformationalVersion;
            
            string port = Env.GetString("LISTENING_PORT");
            string doEndpoint = Env.GetString("DO_ENDPOINT");
            
            Console.WriteLine($"Starting Unisave Compiler {version} ...");
            Console.WriteLine($"Listening on port {port}");
            Console.WriteLine($"Using DO spaces endpoint {doEndpoint}");
            Console.WriteLine("Process ID: " + Process.GetCurrentProcess().Id);
        }
        
        /// <summary>
        /// Stop the script runner server
        /// </summary>
        public void Stop()
        {
            Log.Info("Stopping script runner...");
            
            httpServer?.Stop();
            
            Log.Info("Bye.");
        }

        public void Dispose()
        {
            Stop();
        }
        
        ////////////////////////////
        // Handling HTTP requests //
        ////////////////////////////

        private Task<string> IndexPage(HttpListenerContext context)
        {
            string json = "{'healthy':true}".Replace('\'', '"');
            
            return Task.FromResult(json);
        }
        
        private async Task<CompilationResponse> CompileBackend(
            CompilationRequest request
        )
        {
            // TODO: router needs to handle exceptions and return 500 to client
            // TODO: routes need to return HttpResponse objects
            // TODO: index page shouldn't be basic-auth protected
            
            // TODO: add some S3 package and implement backend downloading
            // TODO: individual compilations need to exclude each other
            
            await Task.Yield(); // dummily awaitify
            
            return new CompilationResponse {
                Success = false,
                Message = "Compiler is not implemented.",
                Output = null
            };
        }
    }
}