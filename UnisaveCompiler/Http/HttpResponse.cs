using System.Net;

namespace UnisaveCompiler.Http
{
    public class HttpResponse
    {
        public string ContentType { get; set; } = null;

        public byte[] Body { get; set; } = null;

        public int StatusCode { get; set; } = 200;

        public void Send(HttpListenerResponse response)
        {
            // NOTE: This method should not throw an exception to make
            // sure the response is formatted closed properly
            
            // set status code
            response.StatusCode = StatusCode;
            
            // send body
            if (ContentType != null && Body != null)
            {
                response.Headers.Add("Content-Type", ContentType);
                response.ContentLength64 = Body.Length;
                response.OutputStream.Write(Body, 0, Body.Length);
                response.OutputStream.Close();
            }
        }
    }
}