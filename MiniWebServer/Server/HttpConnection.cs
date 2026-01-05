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
                        //Console.WriteLine($"[{_connectionId}] Raw Request:\n{rawRequest}");
                        HttpRequest request = RequestParser.Parse(rawRequest);
                        Console.WriteLine($"[{_connectionId}] Request: {request.Method} {request.Url}");

                        string content = "";
                        string statusCode = "200 OK";

                        if (request.Url == "/")
                        {
                            content = "<h1>Home Page</h1><p>Welcome to my Mini Server</p>";
                        }
                        else if (request.Url == "/json")
                        {
                            content = "{ \"message\": \"Hello JSON\", \"status\": \"success\" }";
                        }
                        else if (request.Url.StartsWith("/hello"))
                        {
                            content = $"<h1>Hello User!</h1><p>Your User-Agent is: {GetValue(request, "User-Agent")}</p>";
                        }
                        else
                        {
                            statusCode = "404 Not Found";
                            content = "<h1>404 - Page Not Found</h1>";
                        }

                        string response = $"HTTP/1.1 {statusCode}\r\n" +
                                          "Content-Type: " + (request.Url == "/json" ? "application/json" : "text/html") + "\r\n" +
                                          "Content-Length: " + Encoding.UTF8.GetByteCount(content) + "\r\n" +
                                          "Connection: close\r\n" +
                                          "\r\n" +
                                          content;

                        byte[] responseBytes = Encoding.UTF8.GetBytes(response);
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
            return req.Headers.ContainsKey(key) ? req.Headers[key] : "Unknown";
        }
    }
}
