using System.Net;
using System.Net.Sockets;

namespace MiniWebServer.Server
{
    public class HttpServer
    {
        private readonly TcpListener _tcpListener;
        private readonly int _port;
        private CancellationTokenSource _tokenSource;

        public HttpServer(string ipAddress, int port)
        {
            _port = port;
            _tcpListener = new TcpListener(IPAddress.Parse(ipAddress), _port);
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
                    _ = HandleClientAsync(tcpClient);
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

        private async Task HandleClientAsync(TcpClient tcpClient)
        {
            var connection = new HttpConnection(tcpClient);
            await connection.ConnectAsync();
        }

        public void Stop()
        {
            _tokenSource?.Cancel();
            Console.WriteLine("[Server] Stopped.");
        }
    }
}

