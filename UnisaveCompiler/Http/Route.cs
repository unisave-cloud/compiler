using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace UnisaveCompiler.Http
{
    public abstract class Route
    {
        /// <summary>
        /// Method to match (null = any)
        /// </summary>
        public HttpMethod Method { get; }

        /// <summary>
        /// Url to match
        /// </summary>
        public string Url { get; }
        
        /// <summary>
        /// Token to check for auth
        /// </summary>
        public string SecretToken { get; }
        
        public Route(
            HttpMethod method,
            string url,
            string token
        )
        {
            Method = method;
            Url = url;
            SecretToken = token;
        }
        
        /// <summary>
        /// Returns true if this routes matches a given request
        /// </summary>
        public virtual bool MatchesRequest(HttpListenerRequest request)
        {
            if (Method != null)
            {
                if (request.HttpMethod != Method.Method)
                    return false;
            }

            if (Url != null)
            {
                if (request.Url.AbsolutePath != Url)
                    return false;
            }
            
            return true;
        }

        /// <summary>
        /// Returns true if the given user is allowed to access the route
        /// </summary>
        /// <param name="identity"></param>
        /// <returns></returns>
        public virtual bool Authenticate(HttpListenerBasicIdentity identity)
        {
            if (identity.Name != "api")
                return false;
            
            if (identity.Password != SecretToken)
                return false;

            return true;
        }

        /// <summary>
        /// Invoke the route handler that computes the response
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public abstract Task<HttpResponse> InvokeAsync(
            HttpListenerRequest request
        );
    }
}