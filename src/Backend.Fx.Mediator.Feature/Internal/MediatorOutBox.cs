using System.Collections.Concurrent;
using System.Security.Principal;
using Backend.Fx.Logging;
using Microsoft.Extensions.Logging;

namespace Backend.Fx.Mediator.Feature.Internal;

/// <summary>
/// Enqueues notifications until completion of the current <see cref="Backend.Fx.Execution.Pipeline.IOperation"/>.
/// Requests are handled immediately.
/// </summary>
public class MediatorOutbox : IMediator
{
    private readonly ILogger _logger = Log.Create<MediatorOutbox>();
    private readonly IRootMediator _rootMediator;
    private readonly ConcurrentQueue<Func<CancellationToken, ValueTask>> _outbox = new();

    public MediatorOutbox(IRootMediator rootMediator)
    {
        _rootMediator = rootMediator;
    }

    public ValueTask Notify<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
    {
        _outbox.Enqueue(ct => _rootMediator.Notify(notification, ct));
        return ValueTask.CompletedTask;
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
        => _rootMediator.RequestAsync(request, cancellation);

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
        => _rootMediator.RequestAsync(request, requestor, cancellation);

    public async ValueTask FlushAsync(CancellationToken cancellation)
    {
        while (_outbox.TryDequeue(out var func))
        {
            await func.Invoke(cancellation);
        }
    }

    public void Dispose()
    {
        if (_outbox.IsEmpty == false)
        {
            _logger.LogWarning("Mediator outbox is being disposed but still has {Count} notifications in it.",
                _outbox.Count);
        }
    }

    public ValueTask DisposeAsync()
    {
        Dispose();
        return ValueTask.CompletedTask;
    }
}