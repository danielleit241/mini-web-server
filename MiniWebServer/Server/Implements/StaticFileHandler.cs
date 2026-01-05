using MiniWebServer.Server.Interfaces;

namespace MiniWebServer.Server.Implements
{
    public class StaticFileHandler : IRequestHandler
    {
        private readonly string _rootPath;
        public StaticFileHandler(string rootPath)
        {
            _rootPath = rootPath;
        }
        public HttpResponse HandleRequest(HttpRequest request)
        {
            string urlPath = request.Url == "/" ? "/index.html" : request.Url;
            string filePath = Path.Combine(_rootPath, urlPath.TrimStart('/'));

            HttpResponse response = new HttpResponse();


            if (File.Exists(filePath))
            {
                try
                {
                    byte[] fileBytes = File.ReadAllBytes(filePath);
                    string contentType = GetContentType(filePath);
                    response.SetContent(fileBytes, contentType);
                    response.StatusCode = "200 OK";
                }
                catch (Exception ex)
                {
                    response.StatusCode = "500 Internal Server Error";
                    response.SetContent($"<h1>500 Internal Server Error</h1><p>{ex.Message}</p>");
                }
            }
            else
            {
                response.StatusCode = "404 Not Found";
                response.SetContent("<h1>404 File Not Found</h1>");
            }

            return response;
        }

        private string GetContentType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();
            return extension switch
            {
                ".html" => "text/html; charset=UTF-8",
                ".css" => "text/css",
                ".js" => "application/javascript",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                _ => "application/octet-stream",
            };
        }
    }
}
