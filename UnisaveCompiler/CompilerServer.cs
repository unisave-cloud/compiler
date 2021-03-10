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

        private async Task<HealthCheckResponse> IndexPage(HttpListenerRequest _)
        {
            // TODO: remove this temporary debugging mess
            
            var req = new CompilationRequest() {
                GameId = "9M6kCRcv4V8D82on",
                BackendId = "hvzQRntILRg2XL8I",
                Files = new List<CompilationRequest.BackendFile> {
                    new CompilationRequest.BackendFile { hash = "9b920a8df59c1856a12aea2b6a1c28d0", path = "Assets/UnisaveFixture/Backend/Core/Broadcasting/MyOtherMessage.cs"},
                    new CompilationRequest.BackendFile { hash = "347b5f314ea5d300b21cbdcaeca32811", path = "Assets/UnisaveFixture/Backend/EmailAuthentication/EmailAuthUtils.cs"},
                    new CompilationRequest.BackendFile { hash = "1e36ca175c31c8326d5236b45ad48842", path = "Assets/UnisaveFixture/Backend/Core/Broadcasting/BroadcastingFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "a6f8b351df97082b27140f43f4a604e5", path = "Assets/UnisaveFixture/Backend/Core/Authentication/PlayerEntity.cs"},
                    new CompilationRequest.BackendFile { hash = "9fe7504e7f801ef3b1f81f350ae18e1b", path = "Assets/UnisaveFixture/Backend/SteamMicrotransactions/SteamPurchasingServerFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "d4ec8768d3f7280456e270673938d548", path = "Assets/UnisaveFixture/Backend/Core/Sessions/SessionFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "e96b88a4f929b62b9d3ad0da9ef7a48a", path = "Assets/UnisaveFixture/Backend/EmailAuthentication/EmailLoginFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "f85ba3f466bc5dcc112740ba2687aa87", path = "Assets/UnisaveFixture/Backend/Core/SingleEntityOperations/SeoEntity.cs"},
                    new CompilationRequest.BackendFile { hash = "5336a1c1e83b9eedd313b1d0a4cc9dd0", path = "Assets/UnisaveFixture/Backend/Core/SingleEntityOperations/SeoFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "e26842355037927b56a7e31a38a9e75a", path = "Assets/UnisaveFixture/Backend/EmailAuthentication/EmailRegisterFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "50e7bb150cde876f3f366ded0aa498cb", path = "Assets/UnisaveFixture/Backend/Core/Authentication/AuthenticationFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "46508929a1770c1a4465ed1db1d8050b", path = "Assets/UnisaveFixture/Backend/Core/Broadcasting/MyChannel.cs"},
                    new CompilationRequest.BackendFile { hash = "76a8ebc2b4ff2381c47198495fd02347", path = "Assets/UnisaveFixture/Backend/SteamMicrotransactions/SteamTransactionEntity.cs"},
                    new CompilationRequest.BackendFile { hash = "6e3c3d0362c11817c7e68c0c0b0bbaca", path = "Assets/UnisaveFixture/Backend/ExampleTesting/ExampleTestingFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "197fc149e27366fe3d7b2e59076b820b", path = "Assets/UnisaveFixture/Backend/SteamMicrotransactions/VirtualProducts/ExampleVirtualProduct.cs"},
                    new CompilationRequest.BackendFile { hash = "cc2afa80cda7a5b2229c5afdb9422af8", path = "Assets/UnisaveFixture/Backend/ExampleTesting/ExampleTestingEntity.cs"},
                    new CompilationRequest.BackendFile { hash = "d200a57b09d6b8500eed3227b5c53b6b", path = "Assets/UnisaveFixture/Backend/EmailAuthentication/EmailRegisterResponse.cs"},
                    new CompilationRequest.BackendFile { hash = "da319149b132e893fd2d2c757c07ee42", path = "Assets/UnisaveFixture/Backend/SteamAuthentication/SteamLoginFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "e726ff3969114b472d4d71688c0461af", path = "Assets/UnisaveFixture/Backend/Core/Authentication/SupportFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "ac8c104d1a0770c25a7160cd674e6e1f", path = "Assets/UnisaveFixture/Backend/SteamMicrotransactions/IVirtualProduct.cs"},
                    new CompilationRequest.BackendFile { hash = "961181bdd3322b6ff8a876bf11d7590a", path = "Assets/UnisaveFixture/Backend/Core/Logging/LogFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "6c789ca0ec99bffddf0d9f18df3b0b8e", path = "Assets/UnisaveFixture/Backend/Core/Facets/SomeFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "b791cad7a2093698f3eb15c853481a70", path = "Assets/UnisaveFixture/Backend/Core/FullstackUtilsFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "9db5baf2ce8e2b0b8af4e283eaccd386", path = "Assets/UnisaveFixture/Backend/Core/Arango/RawAqlFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "18dc479da1198c0c8158549021f6bb49", path = "Assets/UnisaveFixture/Backend/Core/Assert.cs"},
                    new CompilationRequest.BackendFile { hash = "e78c5dce4b25bfc10c56771f7a084a2e", path = "Assets/UnisaveFixture/Backend/Core/FacetCalling/FcFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "da97e8f2471493d3e987be95ea6f4e65", path = "Assets/UnisaveFixture/Backend/Core/Broadcasting/MyMessage.cs"},
                    new CompilationRequest.BackendFile { hash = "e0643fffa1c8728defe79f0c3a08dbcb", path = "Assets/UnisaveFixture/Backend/Core/Facets/WrongFacet.cs"},
                    new CompilationRequest.BackendFile { hash = "d4b71db64238a50ed98655d43196b51a", path = "Assets/UnisaveFixture/Backend/Core/Facets/FacetWithMiddleware.cs"},
                    new CompilationRequest.BackendFile { hash = "f3eb1e4bd930309adcfb9c11915c14d5", path = "Assets/UnisaveFixture/Backend/Authentication/PlayerEntity.cs"}
                },
                FrameworkVersion = "0.10.2"
            };
            req.Validate();
            var x = await compiler.CompileBackend(req);
            Log.Info(x.Output);
            Log.Warning(x.Message);
            return new HealthCheckResponse {
                Healthy = x.Success
            };
            
            // TODO: put this original code here instead
//            return Task.FromResult(new HealthCheckResponse {
//                Healthy = true
//            });
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
