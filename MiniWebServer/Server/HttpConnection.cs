using MiniWebServer.Server.Implements;
using MiniWebServer.Server.Interfaces;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace MiniWebServer.Server
{
    public class HttpConnection
    {
        private readonly TcpClient _tcpClient;
        private readonly string _connectionId;
        private readonly IRequestHandler _requestHandler;
        private readonly X509Certificate2 _serverCertificate;

        public HttpConnection(TcpClient tcpClient, X509Certificate2 serverCertificate)
        {
            _tcpClient = tcpClient;
            _connectionId = Guid.NewGuid().ToString().Substring(0, 8);
            _serverCertificate = serverCertificate;

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
                using (var sslStream = new SslStream(networkStream, false))
                {
                    //Cấu hình ALPN để hỗ trợ HTTP/2 và HTTP/1.1
                    var sslOptions = new SslServerAuthenticationOptions
                    {
                        ServerCertificate = _serverCertificate,
                        ClientCertificateRequired = false,
                        ApplicationProtocols = new List<SslApplicationProtocol>
                        {
                            SslApplicationProtocol.Http2,
                            SslApplicationProtocol.Http11
                        }
                    };

                    // Handshake SSL/TLS
                    await sslStream.AuthenticateAsServerAsync(_serverCertificate, false, false);

                    // Xác định giao thức HTTP
                    var protocol = sslStream.NegotiatedApplicationProtocol;
                    if (protocol == SslApplicationProtocol.Http2)
                    {
                        Console.WriteLine("Handling HTTP/2 connection");
                        await HandleRequestHttp2(sslStream);
                    }
                    else if (protocol == SslApplicationProtocol.Http11)
                    {
                        Console.WriteLine("Handling HTTP/1.1 connection");
                        await HandleRequestHttp1(sslStream);
                    }
                    else
                    {
                        Console.WriteLine("Unknown protocol, closing connection.");
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

        private async Task HandleRequestHttp2(SslStream sslStream)
        {
            throw new NotImplementedException();
        }

        private async Task HandleRequestHttp1(SslStream sslStream)
        {
            byte[] buffer = new byte[4096];

            int bytesRead = await sslStream.ReadAsync(buffer, 0, buffer.Length);

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

                await sslStream.WriteAsync(responseBytes, 0, responseBytes.Length);
            }
        }
    }
}
