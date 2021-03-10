using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;

namespace UnisaveCompiler
{
    public class UnisaveFrameworkRepository
    {
        private readonly AmazonS3Client s3Client;
        private readonly string bucket;
        
        public UnisaveFrameworkRepository(
            AmazonS3Client s3Client,
            string bucket
        )
        {
            this.s3Client = s3Client;
            this.bucket = bucket;
            
            if (!Directory.Exists("unisave-framework"))
                Directory.CreateDirectory("unisave-framework");
        }

        public async Task PrepareFramework(string frameworkVersion)
        {
            // download if not present
            if (!Directory.Exists($"unisave-framework/{frameworkVersion}"))
                await DownloadFramework(frameworkVersion);
            
            // or if it's a dev version
            else if (frameworkVersion.Contains("dev"))
                await DownloadFramework(frameworkVersion);
        }

        public IEnumerable<string> GetFrameworkReferences(
            string frameworkVersion
        )
        {
            if (!Directory.Exists("unisave-framework"))
                throw new InvalidOperationException(
                    "You cannot get framework references before first " +
                    "preparing the framework version."
                );
            
            // go through all files and return compiler references to all DLLs
            var fileNames = Directory.GetFiles(
                $"unisave-framework/{frameworkVersion}"
            );
            
            foreach (string fileName in fileNames)
            {
                // skip non-dll files
                if (Path.GetExtension(fileName).ToLowerInvariant() != ".dll")
                    continue;

                yield return $"-reference:{fileName}";
            }
        }

        private async Task DownloadFramework(
            string frameworkVersion
        )
        {
            Log.Info($"Downloading framework '{frameworkVersion}'...");

            // trailing slash is important as it's literally a string prefix,
            // it would match even '-dev' versions otherwise, etc...
            string prefix = $"unisave-framework/{frameworkVersion}/";

            ListObjectsResponse response = await s3Client.ListObjectsAsync(
                bucket, prefix
            );
            
            foreach (S3Object obj in response.S3Objects)
            {
                string fileName = obj.Key.Substring(prefix.Length);
                
                Log.Debug($"Downloading '{fileName}'...");
                
                GetObjectResponse r = await s3Client.GetObjectAsync(
                    bucket,
                    obj.Key
                );
                
                await r.WriteResponseStreamToFileAsync(
                    filePath: $"unisave-framework/{frameworkVersion}/{fileName}",
                    append: false,
                    cancellationToken: new CancellationToken()
                );
            }
        }
    }
}