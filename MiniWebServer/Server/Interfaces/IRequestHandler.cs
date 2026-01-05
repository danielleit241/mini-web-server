namespace MiniWebServer.Server.Interfaces
{
    public interface IRequestHandler
    {
        HttpResponse HandleRequest(HttpRequest request);
    }
}
