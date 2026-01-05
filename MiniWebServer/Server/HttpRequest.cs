namespace MiniWebServer.Server
{
    public class HttpRequest
    {
        public string Method { get; set; }
        public string Url { get; set; }
        public string Version { get; set; }
        public Dictionary<string, string> Headers { get; set; }
        public string Body { get; set; }

        public HttpRequest()
        {
            Headers = new Dictionary<string, string>();
            Method = "GET"; // Default method
            Url = "/";
            Version = "HTTP/1.1";
        }
    }
}
