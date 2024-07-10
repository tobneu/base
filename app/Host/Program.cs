using Grpc.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WebServices.GrpcServices;
using WebServices.Protos;

public class Program
{
    public static async Task Main(string[] args)
    {
        var host = new HostBuilder()
            .ConfigureAppConfiguration((context, config) =>
            {
                config.SetBasePath(Directory.GetCurrentDirectory());
                //config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
            })
            .ConfigureServices((context, services) =>
            {
                services.AddGrpc();
                services.AddSingleton<ChatServiceImplementation>();
                services.AddSingleton<ChatCache>();
            })
            .ConfigureLogging(logging => logging.AddConsole())
            .Build();

        var server = new Server
        {
            Services = 
            {
                ChatService.BindService(host.Services.GetRequiredService<ChatServiceImplementation>())
            },
            Ports = { new ServerPort("localhost", 5000, ServerCredentials.Insecure) }
        };

        server.Start();

        Console.WriteLine("gRPC server listening on port 5000");
        Console.WriteLine("Press any key to stop the server...");
        Console.ReadKey();

        await server.ShutdownAsync();
    }
}