using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace UnisaveCompiler.Http
{
    /// <summary>
    /// Passes a given HTTP request to an appropriate handler method
    /// </summary>
    public class Router
    {
        private readonly List<Route> routes = new List<Route>();

        private readonly Route defaultRoute = new JsonRoute<Response404>(
            null, null, _ => Task.FromResult(new Response404()), null
        );

        public void AddRoute(Route route)
        {
            routes.Add(route);
        }

        public Route ResolveRoute(HttpListenerRequest request)
        {
            foreach (var route in routes)
            {
                if (route.MatchesRequest(request))
                    return route;
            }

            return defaultRoute;
        }
    }
}