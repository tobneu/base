using System.Collections.Concurrent;
using WebServices.Protos;
using Grpc.Core;

namespace WebServices.GrpcServices;

public class ChatServiceImplementation : ChatService.ChatServiceBase {
    private static readonly ConcurrentDictionary<IServerStreamWriter<ChatMessage>, bool> Subscribers = new();
    private readonly ChatCache _cache;

    public ChatServiceImplementation(ChatCache cache) {
        _cache = cache;
        // add 3 test messages to the cache
        _cache.AddMessage(new ChatMessage { Message = "Hello" });
        _cache.AddMessage(new ChatMessage { Message = "World" });
        _cache.AddMessage(new ChatMessage { Message = "From the cache" });
    }

    public override async Task StreamChat(Empty request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context) {
        Console.WriteLine("Stream Chat");
        Subscribers.TryAdd(responseStream, true);

        while (!context.CancellationToken.IsCancellationRequested) {
            while (_cache.TryDequeue(out var message)) {
                foreach (var subscriber in Subscribers.Keys) {
                    await subscriber.WriteAsync(message);
                }
            }
            await Task.Delay(100); // Prevent tight loop
        }
        
        Subscribers.TryRemove(responseStream, out _);
    }

    // Todo: Remove this method
    public override Task<Empty> TestChatMessage(ChatMessage request, ServerCallContext context) {
        // append message content to the cache 
        Console.WriteLine($"Received Message {request.Message}");
        _cache.AddMessage(request);
        return Task.FromResult(new Empty());
    }

    // Todo: Remove this method
    public override async Task<HelloWorldResponse> HelloWorld(Empty request, ServerCallContext context) {

        await Task.Delay(1000);
        return new HelloWorldResponse() {
            Message = "Hi"
        };
    }

    // Todo: Remove this method
    public override async Task HelloStream(Empty request, IServerStreamWriter<HelloWorldResponse> responseStream, ServerCallContext context) {
        var random = new Random();
        while (!context.CancellationToken.IsCancellationRequested)
        {
            var randomNumber = random.Next(); // Generates a random number
            await responseStream.WriteAsync(new HelloWorldResponse { Message = randomNumber.ToString() });
            await Task.Delay(100); // Delay for 1 second before sending the next number
        }
    }
}