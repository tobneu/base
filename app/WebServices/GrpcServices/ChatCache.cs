using System.Collections.Concurrent;
using app.GrpcServices;

namespace WebServices.GrpcServices;

public class ChatCache
{
    private static readonly Lazy<ChatCache> _instance = new(() => new ChatCache());
    public static ChatCache Instance => _instance.Value;

    private readonly ConcurrentQueue<ChatMessage> _messages = new();

    private ChatCache() {}

    public void AddMessage(ChatMessage message)
    {
        _messages.Enqueue(message);
    }

    public IEnumerable<ChatMessage> GetMessages()
    {
        return _messages.ToArray();
    }

    public bool TryDequeue(out ChatMessage message)
    {
        return _messages.TryDequeue(out message);
    }
}