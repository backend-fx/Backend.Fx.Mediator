using System.Collections.Concurrent;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.Outbox;

internal class MediatorOutbox : IMediatorOutbox
{
    private readonly ILogger _logger = Log.Create<MediatorOutbox>();
    private readonly ConcurrentQueue<Func<CancellationToken, ValueTask>> _outbox = new();
    
    public async ValueTask FlushAsync(CancellationToken cancellation)
    {
        while (_outbox.TryDequeue(out var func))
        {
            await func.Invoke(cancellation);
        }
    }

    public void Enqueue(Func<CancellationToken, ValueTask> notification)
    {
        _outbox.Enqueue(notification);
    }
    
    public void Dispose()
    {
        if (_outbox.IsEmpty == false)
        {
            _logger.LogWarning(
                "Mediator outbox is being disposed but still has {Count} notifications in it.",
                _outbox.Count);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}