using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnisaveCompiler.Http;

namespace UnisaveCompiler
{
    public class CompilationRequest
    {
        [JsonProperty("game_id")]
        public string GameId { get; set; }
        
        [JsonProperty("backend_id")]
        public string BackendId { get; set; }
        
        [JsonProperty("files")]
        public List<BackendFile> Files { get; set; }
        
        [JsonProperty("framework_version")]
        public string FrameworkVersion { get; set; }

        public struct BackendFile
        {
            [JsonProperty("path")]
            public string path;
            
            [JsonProperty("hash")]
            public string hash;
        }

        /// <summary>
        /// Make sure the request contains valid data
        /// </summary>
        public void Validate()
        {
            if (GameId.Contains("/") || GameId.Contains("\\"))
                throw new ValidationException(
                    "Game ID cannot contain slashes"
                );
            
            if (BackendId.Contains("/") || BackendId.Contains("\\"))
                throw new ValidationException(
                    "Backend ID cannot contain slashes"
                );
            
            var paths = new HashSet<string>();
            
            foreach (var file in Files)
            {
                if (file.path.StartsWith("/"))
                    throw new ValidationException(
                        "Backend file paths must be relative"
                    );
                
                if (file.path.Contains("\\"))
                    throw new ValidationException(
                        "Backend file paths must be in the unix style"
                    );
                
                if (file.path.Length > 2048)
                    throw new ValidationException(
                        "Backend file paths must be at most 2048 chars long"
                    );
                
                if (file.hash.Length > 64)
                    throw new ValidationException(
                        "Backend file hashes must be at most 64 chars long"
                    );
                
                if (file.hash.Contains("/") || file.hash.Contains("\\"))
                    throw new ValidationException(
                        "File hashes cannot contain slashes"
                    );

                if (paths.Contains(file.path))
                    throw new ValidationException(
                        $"File has been provided multiple times: {file.path}"
                    );
                
                paths.Add(file.path);
            }
            
            if (FrameworkVersion.Contains("/") || FrameworkVersion.Contains("\\"))
                throw new ValidationException(
                    "Framework version cannot contain slashes"
                );
        }
    }
}