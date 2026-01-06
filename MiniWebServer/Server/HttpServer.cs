using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;

namespace MiniWebServer.Server
{
    public class HttpServer
    {
        private readonly TcpListener _tcpListener;
        private readonly int _port;
        private CancellationTokenSource _tokenSource;
        private readonly X509Certificate2 _serverCertificate;

        public HttpServer(string ipAddress, int port, X509Certificate2 serverCertificate)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), _port);
            _serverCertificate = serverCertificate;
        }

        public async Task StartAsync()
        {
            _tcpListener.Start();
            _tokenSource = new CancellationTokenSource();
            Console.WriteLine("[Server] Started on port " + _port);
            try
            {
                while (!_tokenSource.Token.IsCancellationRequested)
                {
                    // Chờ kết nối từ client
                    var tcpClient = await _tcpListener.AcceptTcpClientAsync();

                    // Xử lý kết nối trong một task riêng
                    _ = HandleClientAsync(tcpClient, _serverCertificate);
                }

            }
            catch (OperationCanceledException)
            {
                Console.WriteLine("[Server] Stopping listener...");
            }
            finally
            {
                _tcpListener.Stop();
            }
        }

        private async Task HandleClientAsync(TcpClient tcpClient, X509Certificate2 serverCertificate)
        {
            var connection = new HttpConnection(tcpClient, serverCertificate);
            await connection.ConnectAsync();
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            Console.WriteLine("[Server] Stopped.");
        }
    }
}

