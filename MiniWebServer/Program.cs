using MiniWebServer.Server;

class Program
{
    static async Task Main(string[] args)
    {
        var server = new HttpServer("127.0.0.1", 8080);

        var serverTask = server.StartAsync();

        Console.WriteLine("Press Enter to stop the server...");
        Console.ReadLine();

        Console.WriteLine("Stopping server...");
        server.Stop();

        await serverTask;
        Console.WriteLine("Server stopped.");
    }
}