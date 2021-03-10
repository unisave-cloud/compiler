using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace UnisaveCompiler
{
    /// <summary>
    /// The service responsible for compiling backends
    /// </summary>
    public class Compiler
    {
        private readonly AmazonS3Client s3Client;
        private readonly string bucket;

        private readonly ThreadSafeCounter counter;
        private readonly UnisaveFrameworkRepository frameworkRepository;
        
        public Compiler(AmazonS3Client s3Client, string bucket)
        {
            this.s3Client = s3Client;
            this.bucket = bucket;
            
            counter = new ThreadSafeCounter();
            frameworkRepository = new UnisaveFrameworkRepository(
                s3Client,
                bucket
            );

            if (Directory.Exists("compilation"))
                Directory.Delete("compilation", true);
            Directory.CreateDirectory("compilation");
        }

        public async Task<CompilationResponse> CompileBackend(
            CompilationRequest request
        )
        {
            var sw = new Stopwatch();
            sw.Start();
            
            int number = counter.GetNext();
            
            Log.Info($"Starting compilation number {number}, of backend " +
                     $"{request.BackendId} for game {request.GameId}...");

            await frameworkRepository.PrepareFramework(request.FrameworkVersion);
            PrepareCompilationDirectory(number);
            await DownloadBackendFiles(number, request.GameId, request.Files);
            var response = await RunTheCscCompiler(number, request);
            if (response.Success)
                await UploadCompiledFiles(number, request);
            //RemoveCompilationDirectory(number); // TODO: uncomment this!

            sw.Stop();
            double secs = Math.Round(sw.Elapsed.TotalSeconds, 3);
            Log.Info($"Compilation number {number} has finished in {secs} seconds!");
            
            return response;
        }

        private void PrepareCompilationDirectory(int number)
        {
            string path = $"compilation/{number}";
            
            if (Directory.Exists(path))
                throw new Exception(
                    $"Compilation directory {number} already exists."
                );

            Directory.CreateDirectory(path);
        }

        private async Task DownloadBackendFiles(
            int number,
            string gameId,
            List<CompilationRequest.BackendFile> files
        )
        {
            Log.Info("Downloading backend files...");
            
            string ga = gameId.Substring(0, 2);

            foreach (var file in files)
            {
                Log.Debug($"Downloading {file.hash}: {file.path}");
                
                string ha = file.hash.Substring(0, 2);
                
                GetObjectResponse response = await s3Client.GetObjectAsync(
                    bucket,
                    $"games/{ga}/{gameId}/backend-files/{ha}/{file.hash}"
                );
                await response.WriteResponseStreamToFileAsync(
                    filePath: $"compilation/{number}/{file.path}",
                    append: false,
                    cancellationToken: new CancellationToken()
                );
            }

            Log.Info("All backend files downloaded.");
        }

        private IEnumerable<string> GetCompilerArguments(
            int number,
            CompilationRequest request
        )
        {
            string compilationPath = $"compilation/{number}";
            
            // === constant flags ===
            
            yield return "-target:library";
            yield return "-platform:anycpu";
            yield return "-optimize+";
            yield return "-utf8output";
            
            // output file
            yield return $"-out:{compilationPath}/backend.dll";
            
            // generate .pdb file
            yield return "-debug+";
            yield return $"-pdb:{compilationPath}/backend.pdb";

            // source code path mapping
            yield return "-fullpaths";
            yield return $"-pathmap:{compilationPath}=[Server]";

            // === user-specified flags ===
            
            // TODO ...
            
            // c# version
            // .NET version
            // checked?
            // safe?
            // #define
            // warn as error
            
            // === references ===

            // Unisave Framework references
            var frameworkRefs = frameworkRepository.GetFrameworkReferences(
                request.FrameworkVersion
            );
            foreach (string r in frameworkRefs)
                yield return r;
            
            // .dll libraries within the backend files
            foreach (var file in request.Files)
            {
                if (Path.GetExtension(file.path)?.ToLowerInvariant() == ".dll")
                    yield return $"-reference:{compilationPath}/{file.path}";
            }
            
            // === source code files ===
            
            foreach (var file in request.Files)
            {
                if (Path.GetExtension(file.path)?.ToLowerInvariant() == ".cs")
                    yield return $"{compilationPath}/{file.path}";
            }
        }
        
        private Task<CompilationResponse> RunTheCscCompiler(
            int number,
            CompilationRequest request
        )
        {
            // === prepare process parameters ===

            string cscArguments = string.Join(
                " ",
                GetCompilerArguments(number, request)
            );
            
            var proc = new Process {
                EnableRaisingEvents = true,
                StartInfo = {
                    UseShellExecute = false,
                    FileName = "csc",
                    Arguments = cscArguments,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };
            
            // === compiler output logging ===

            const int maxCompilerOutput = 1024 * 1024; // 1 MB
            var compilerOutput = new StringBuilder(
                capacity: 1024 * 10, // 10 KB
                maxCapacity: maxCompilerOutput
            );

            proc.OutputDataReceived += (s, e) => {
                compilerOutput.AppendLine(e.Data);
            };
            proc.ErrorDataReceived += (s, e) => {
                compilerOutput.AppendLine(e.Data);
            };
            
            // === result handling ===
            
            var tcs = new TaskCompletionSource<CompilationResponse>();
            
            proc.Exited += (sender, args) => {
                bool success = proc.ExitCode == 0;
                var output = new CompilationResponse {
                    Success = success,
                    Output = compilerOutput.ToString(),
                    Message = success ?
                        "Compilation was successful." :
                        "Compilation error."
                };

                proc.Dispose();
                
                tcs.SetResult(output);
            };
            
            // === bootstrap ===
            
            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();

            return tcs.Task;
        }

        private async Task UploadCompiledFiles(
            int number,
            CompilationRequest request
        )
        {
            Log.Info("Uploading compiled files...");
            
            string[] filesToUpload = {
                "backend.dll",
                "backend.pdb"
            };
            
            string ga = request.GameId.Substring(0, 2);

            foreach (string file in filesToUpload)
            {
                Log.Debug($"Uploading '{file}'...");
                
                await s3Client.PutObjectAsync(
                    new PutObjectRequest {
                        FilePath = $"compilation/{number}/{file}",
                        BucketName = bucket,
                        Key = $"games/{ga}/{request.GameId}/backends/" +
                              $"{request.BackendId}/{file}"
                    }
                );
            }
        }
        
        private void RemoveCompilationDirectory(int number)
        {
            string path = $"compilation/{number}";
            
            if (!Directory.Exists(path))
                throw new Exception(
                    $"Compilation directory {number} doesn't exist, " +
                    $"so cannot be removed."
                );
            
            Directory.Delete(path, true);
        }
    }
}