using System.Collections.Concurrent;
using app.GrpcServices;
using Grpc.Core;
using Empty = Google.Protobuf.WellKnownTypes.Empty;

namespace WebServices.GrpcServices;

public class ChatServiceImplementation : ChatService.ChatServiceBase
{
    private static readonly ConcurrentDictionary<IServerStreamWriter<ChatMessage>, bool> _subscribers = new ConcurrentDictionary<IServerStreamWriter<ChatMessage>, bool>();

    public override async Task StreamChat(app.GrpcServices.Empty request, IServerStreamWriter<ChatMessage> responseStream, ServerCallContext context) {
        _subscribers.TryAdd(responseStream, true);

        // Stream messages from the cache to the client
        while (!context.CancellationToken.IsCancellationRequested)
        {
            if (ChatCache.Instance.TryDequeue(out var message))
            {
                foreach (var subscriber in _subscribers.Keys)
                {
                    await subscriber.WriteAsync(message);
                }
            }

            await Task.Delay(100); // Prevent tight loop
        }

        _subscribers.TryRemove(responseStream, out _);
    }
}