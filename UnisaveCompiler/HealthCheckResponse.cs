using Newtonsoft.Json;

namespace UnisaveCompiler
{
    public class HealthCheckResponse
    {
        [JsonProperty("healthy")]
        public bool Healthy { get; set; }
    }
}