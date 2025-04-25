using System.Collections.Concurrent;
using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature;

public interface IMediatorOutbox
{
    void Flush();
}

public class MediatorOutbox : IMediator, IMediatorOutbox
{
    private readonly IRootMediator _mediator;
    private readonly ConcurrentQueue<Action> _outbox = new();

    public MediatorOutbox(IRootMediator mediator)
    {
        _mediator = mediator;
    }

    public void Notify<TNotification>(TNotification notification) where TNotification : class
    {
        _outbox.Enqueue(() => _mediator.Notify(notification));
    }

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse> 
        => _mediator.RequestAsync<TRequest, TResponse>(request, cancellation);

    public ValueTask<TResponse> RequestAsync<TRequest, TResponse>(TRequest request, IIdentity requestor,
        CancellationToken cancellation = default) where TRequest : IRequest<TResponse>
        => _mediator.RequestAsync<TRequest, TResponse>(request, requestor, cancellation);
    
    public ValueTask DisposeAsync() => _mediator.DisposeAsync();

    public void Flush()
    {
        foreach (var action in _outbox)
        {
            action.Invoke();
        }
    }
}
