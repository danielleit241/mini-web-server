namespace MiniWebServer.Server
{
    public static class RequestParser
    {
        public static HttpRequest Parse(string rawData)
        {
            var request = new HttpRequest();
            if (string.IsNullOrWhiteSpace(rawData))
            {
                return request;
            }

            //Header và Body phân tách nhau bởi \r\n\r\n
            var parts = rawData.Split(new[] { "\r\n\r\n" }, 2, StringSplitOptions.None);

            string headerSection = parts[0];
            string bodySection = parts.Length > 1 ? parts[1] : string.Empty;

            // Phân tích Header
            var lines = headerSection.Split(new[] { "\r\n" }, StringSplitOptions.None);

            var requestLineParts = lines[0].Split(' ');
            if (requestLineParts.Length >= 2)
            {
                request.Method = requestLineParts[0]; // GET, POST, etc.
                request.Url = requestLineParts[1]; // /path/resource
                request.Version = requestLineParts.Length > 2 ? requestLineParts[2] : "HTTP/1.1"; // HTTP version
            }

            for (int i = 1; i < lines.Length; i++)
            {
                var line = lines[i];
                if (string.IsNullOrWhiteSpace(line)) continue;

                var separatorIndex = line.IndexOf(':');
                if (separatorIndex >= 0)
                {
                    var headerName = line.Substring(0, separatorIndex).Trim();
                    var headerValue = line.Substring(separatorIndex + 1).Trim();

                    if (!request.Headers.ContainsKey(headerName))
                    {
                        request.Headers[headerName] = headerValue;
                    }
                }
            }
            request.Body = bodySection;

            return request;
        }
    }
}
