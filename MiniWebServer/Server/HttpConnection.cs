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
                        response.Headers.Add("Content-Type", "text/html; charset=UTF-8");

                        if (request.Url == "/")
                        {
                            response.Body = "<h1>Welcome to MiniWebServer!</h1><p>This is the home page.</p>";
                            response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                        }
                        else if (request.Url == "/json")
                        {
                            response.Body = "{ \"message\": \"Hello JSON\", \"status\": \"success\" }";
                            response.Headers["Content-Type"] = "application/json";
                        }
                        else if (request.Url.StartsWith("/hello"))
                        {
                            response.Body = $"<h1>Hello User!</h1><p>Your User-Agent is: {GetValue(request, "User-Agent")}</p>";
                            response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                        }
                        else
                        {
                            response.StatusCode = "404 Not Found";
                            response.Body = "<h1>404 - Page Not Found</h1>";
                            response.Headers["Content-Type"] = "text/html; charset=UTF-8";
                        }

                        int bodyLength = Encoding.UTF8.GetByteCount(response.Body);
                        response.Headers["Content-Length"] = bodyLength.ToString();
                        response.Headers["Connection"] = "close";

                        string responseString = response.ToString();
                        byte[] responseBytes = Encoding.UTF8.GetBytes(responseString);
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
    }
}
