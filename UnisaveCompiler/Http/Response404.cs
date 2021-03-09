using Newtonsoft.Json;

namespace UnisaveCompiler.Http
{
    public class Response404
    {
        [JsonProperty("message")]
        public string Message => "404 - Page not found";
    }
}