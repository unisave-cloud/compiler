using System;
using Newtonsoft.Json;

namespace UnisaveCompiler.Http
{
    public class ExceptionResponse
        : JsonResponse<ExceptionResponse.ExceptionResponseBody>
    {
        public class ExceptionResponseBody
        {
            [JsonProperty("success")]
            public bool Success { get; set; }
            
            [JsonProperty("exception")]
            public string Exception { get; set; }
            
            [JsonProperty("message")]
            public string Message { get; set; }
        }
        
        public ExceptionResponse(Exception e)
            : base(new ExceptionResponseBody {
                Success = false,
                Exception = e.GetType().FullName,
                Message = e.ToString()
            }, 500) { }
    }
}