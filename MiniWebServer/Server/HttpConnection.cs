using MiniWebServer.Server.Implements;
using MiniWebServer.Server.Interfaces;
using System.Net.Sockets;
using System.Text;

namespace MiniWebServer.Server
{
    public class HttpConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly string _connectionId;
        private readonly IRequestHandler _requestHandler;

        public HttpConnection(TcpClient tcpClient)
        {
            _tcpClient = tcpClient;
            _connectionId = Guid.NewGuid().ToString().Substring(0, 8);
            string wwwroot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
            _requestHandler = new RoutingHandler(wwwroot);
        }

        public async Task ConnectAsync()
        {
            Console.WriteLine($"Connected: {_connectionId}");
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
                        Console.WriteLine($"Request: {request.Method} {request.Url}");

                        HttpResponse response = _requestHandler.HandleRequest(request);
                        response.Headers["Connection"] = "close";
                        response.Headers["X-Connection-Id"] = _connectionId;
                        byte[] responseBytes = response.ToBytes();
                        Console.WriteLine($"Response: {response.StatusCode}");
                        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
            finally
            {
                Console.WriteLine($"Disconnected: {_connectionId}");
            }
        }
    }
}
