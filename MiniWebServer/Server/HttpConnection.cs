using System.Net.Sockets;
using System.Text;

namespace MiniWebServer.Server
{
    public class HttpConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly string _connectionId;

        public HttpConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _connectionId = Guid.NewGuid().ToString().Substring(0, 8);
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($"[Connection {_connectionId}] Established.");
            try
            {
                using (_tcpClient)
                using (var networkStream = _tcpClient.GetStream())
                {
                    byte[] buffer = new byte[4096];

                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string rawRequest = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        HttpRequest request = RequestParser.Parse(rawRequest);

                        var response = new HttpResponse("200 OK");
                        string wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                        if (request.Url == "/")
                        {
                            response.SetContent("<h1>Home Page</h1>", "text/html; charset=UTF-8");
                        }
                        else
                        {
                            string filePath = Path.Combine(wwwroot, request.Url.TrimStart('/'));

                            if (File.Exists(filePath))
                            {
                                byte[] fileBytes = File.ReadAllBytes(filePath);
                                string contentType = GetContentType(filePath);
                                response.SetContent(fileBytes, contentType);
                            }
                            else
                            {
                                response.StatusCode = "404 Not Found";
                                response.SetContent("<h1>404 File Not Found</h1>");
                            }
                        }

                        response.Headers["Connection"] = "close";

                        byte[] responseBytes = response.ToBytes();
                        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[{_connectionId}] Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"[{_connectionId}] Disconnected.");
            }
        }

        private string GetValue(HttpRequest req, string key)
        {
            foreach (var header in req.Headers)
            {
                if (string.Equals(header.Key, key, StringComparison.OrdinalIgnoreCase))
                {
                    return header.Value;
                }
            }
            return "Unknown";
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
