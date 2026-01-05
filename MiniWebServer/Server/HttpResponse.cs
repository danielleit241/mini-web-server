using System.Text;

namespace MiniWebServer.Server
{
    public class HttpResponse
    {
        public string StatusCode { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public string Body { get; set; } = string.Empty;

        public HttpResponse(string statusCode = "200 OK")
        {
            StatusCode = statusCode;
        }

        public override string ToString()
        {
            StringBuilder responseBuilder = new StringBuilder();

            responseBuilder.AppendLine($"HTTP/1.1 {StatusCode}");

            foreach (var header in Headers)
            {
                responseBuilder.AppendLine($"{header.Key}: {header.Value}");
            }

            responseBuilder.Append("\r\n");

            responseBuilder.Append(Body);

            return responseBuilder.ToString();
        }
    }
}
