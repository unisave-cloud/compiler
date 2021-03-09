using Newtonsoft.Json;

namespace UnisaveCompiler
{
    public class CompilationResponse
    {
        [JsonProperty("success")]
        public bool Success { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("output")]
        public string Output { get; set; }
    }
}