using System.Text;

namespace MiniWebServer.Server
{
    public class HttpResponse
    {
        public string StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public byte[] Body { get; private set; } = [];

        public HttpResponse(string statusCode = "200 OK")
        {
            StatusCode = statusCode;
        }

        public void SetContent(string content, string contentType = "text/html; charset=UTF-8")
        {
            Body = Encoding.UTF8.GetBytes(content);
            Headers["Content-Type"] = contentType;
            Headers["Content-Length"] = Body.Length.ToString();
        }

        public void SetContent(byte[] content, string contentType)
        {
            Body = content;
            Headers["Content-Type"] = contentType;
            Headers["Content-Length"] = Body.Length.ToString();
        }

        public byte[] ToBytes()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine($"HTTP/1.1 {StatusCode}");

            foreach (var header in Headers)
            {
                stringBuilder.Append($"{header.Key}: {header.Value}\r\n");
            }

            stringBuilder.Append("\r\n");

            byte[] headerBytes = Encoding.UTF8.GetBytes(stringBuilder.ToString());
            byte[] responseBytes = new byte[headerBytes.Length + Body.Length];
            Array.Copy(headerBytes, 0, responseBytes, 0, headerBytes.Length);
            Array.Copy(Body, 0, responseBytes, headerBytes.Length, Body.Length);

            return responseBytes;
        }
    }
}
