using MiniWebServer.Server;
using System.Security.Cryptography.X509Certificates;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("Starting MiniWebServer...");

        var certificate = new X509Certificate2("server.pfx", "123456", X509KeyStorageFlags.Exportable);

        var server = new HttpServer("127.0.0.1", 8080, certificate);

        await server.StartAsync();
    }
}