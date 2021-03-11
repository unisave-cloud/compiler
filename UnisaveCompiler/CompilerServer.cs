using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Transfer;
using DotNetEnv;
using UnisaveCompiler.Http;

namespace UnisaveCompiler
{
    /// <summary>
    /// Script runner server that accepts the HTTP requests
    /// </summary>
    public class CompilerServer : IDisposable
    {
        private readonly AmazonS3Client s3Client;
        private readonly string bucket;

        private readonly Compiler compiler;
        
        private readonly HttpServer httpServer;

        public CompilerServer()
        {
            // === setup cloud storage connection ===
            
            // it needs to be passed via env vars for some reason...
            Environment.SetEnvironmentVariable(
                "AWS_ACCESS_KEY_ID",
                Env.GetString("DO_ACCESS_KEY_ID")
            );
            Environment.SetEnvironmentVariable(
                "AWS_SECRET_ACCESS_KEY",
                Env.GetString("DO_SECRET_ACCESS_KEY")
            );
            s3Client = new AmazonS3Client(
                new AmazonS3Config {
                    ForcePathStyle = true,
                    ServiceURL = Env.GetString("DO_ENDPOINT")
                }
            );
            bucket = Env.GetString("DO_BUCKET");
        
            // === setup the compiler service ===
            
            compiler = new Compiler(s3Client, bucket);
            
            // === setup HTTP server ===
            
            var router = new Router();
            
            httpServer = new HttpServer(
                Env.GetInt("LISTENING_PORT"),
                router
            );
            
            // === define HTTP routes ===

            var secretToken = Env.GetString("SECRET_TOKEN");

            router.AddRoute(
                new JsonRoute<HealthCheckResponse>(
                    method: HttpMethod.Get,
                    url: "/",
                    handler: IndexPage,
                    token: null
                )
            );
            
            router.AddRoute(
                new JsonRoute<CompilationRequest, CompilationResponse>(
                    method: HttpMethod.Post,
                    url: "/compile-backend",
                    handler: CompileBackend,
                    token: secretToken
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
        /// Stop the compiler server
        /// </summary>
        public void Stop()
        {
            Log.Info("Stopping the compiler service...");
            
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

        private Task<HealthCheckResponse> IndexPage(HttpListenerRequest _)
        {
            return Task.FromResult(new HealthCheckResponse {
                Healthy = true
            });
        }
        
        private async Task<CompilationResponse> CompileBackend(
            CompilationRequest request,
            HttpListenerRequest _
        )
        {
            request.Validate();
            
            return await compiler.CompileBackend(request);
        }
    }
}
