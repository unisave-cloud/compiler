using System.Collections.Generic;
using Newtonsoft.Json;

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
    }
}