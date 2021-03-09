using System;
using System.Text;
using Newtonsoft.Json;

namespace UnisaveCompiler.Http
{
    public class JsonResponse<TBody> : HttpResponse
    {
        public JsonResponse(TBody body, int status = 200)
        {
            if (body == null)
                throw new ArgumentNullException(nameof(body));
            
            StatusCode = status;
            ContentType = "application/json";
            
            var bodyString = JsonConvert.SerializeObject(body);
            Body = Encoding.UTF8.GetBytes(bodyString);
        }
    }
}