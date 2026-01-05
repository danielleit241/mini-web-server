using MiniWebServer.Server.Interfaces;

namespace MiniWebServer.Server.Implements
{
    public class RoutingHandler : IRequestHandler
    {
        private readonly IRequestHandler _staticFileHandler;
        private readonly IRequestHandler _apiHandler;

        public RoutingHandler(string wwwroot)
        {
            _staticFileHandler = new StaticFileHandler(wwwroot);
            _apiHandler = new ApiHandler();
        }

        public HttpResponse HandleRequest(HttpRequest request)
        {
            if (IsApiRequest(request))
            {
                Console.WriteLine("Routing to API Handler");
                return _apiHandler.HandleRequest(request);
            }

            Console.WriteLine("Routing to Static File Handler");
            return _staticFileHandler.HandleRequest(request);
        }

        private bool IsApiRequest(HttpRequest request)
        {
            return request.Url.StartsWith("/api") || request.Url == "/json" || request.Url.StartsWith("/hello");
        }
    }
}
