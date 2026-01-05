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
                    byte[] buffer = new byte[1024];

                    int bytesRead = await networkStream.ReadAsync(buffer, 0, buffer.Length);

                    if (bytesRead > 0)
                    {
                        string requestData = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                        Console.WriteLine($"[Connection {_connectionId}] Received data:\n{requestData}");

                        string html = "<html><body><h1>Hello from MiniWebServer!</h1></body></html>";
                        var sb = new StringBuilder();
                        sb.Append("HTTP/1.1 200 OK\r\n");
                        sb.Append("Content-Type: text/html\r\n");
                        sb.Append($"Content-Length: {Encoding.UTF8.GetByteCount(html)}\r\n");
                        sb.Append("Connection: close\r\n");
                        sb.Append("\r\n");
                        sb.Append(html);

                        byte[] responseBytes = Encoding.UTF8.GetBytes(sb.ToString());
                        await networkStream.WriteAsync(responseBytes, 0, responseBytes.Length);
                        Console.WriteLine($"[Connection {_connectionId}] Sent response.");
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
    }
}
