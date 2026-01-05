using MiniWebServer.Server.Interfaces;

namespace MiniWebServer.Server.Implements
{
    public class ApiHandler : IRequestHandler
    {

        public HttpResponse HandleRequest(HttpRequest request)
        {
            HttpResponse response = new HttpResponse();
            if (request.Url == "/json")
            {
                response.SetContent("{\"message\": \"Hello, World!\"}", "application/json; charset=UTF-8");
            }
            else if (request.Url.StartsWith("/hello"))
            {
                response.SetContent($"<html><body><h1>Hello from ApiHandler!</h1></body></html>");
            }
            else
            {
                response.StatusCode = "404 Not Found";
                response.SetContent("<html><body><h1>404 Not Found</h1></body></html>", "text/html; charset=UTF-8");
            }

            return response;
        }
    }
}
