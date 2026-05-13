using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature.Outbox;

/// <summary>
/// Enqueues notifications until completion of the current <see cref="Backend.Fx.Execution.Pipeline.IOperation"/>.
/// Requests are handled immediately.
/// </summary>
internal class WithOutbox : IMediator
{
    private readonly IMediator _mediator;
    private readonly IMediatorOutbox _outbox;

    public WithOutbox(IMediator mediator, IMediatorOutbox outbox)
    {
        _mediator = mediator;
        _outbox = outbox;
    }


    public Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
    {
        _outbox.Enqueue(ct => _mediator.NotifyAsync(notification, ct));
        return Task.CompletedTask;
    }

    public Task NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
    {
        _outbox.Enqueue(ct => _mediator.NotifyAsync(notification, notifier, ct));
        return Task.CompletedTask;
    }

    public Task NotifyAsync<TNotification>(TNotification notification, INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
    {
        _outbox.Enqueue(ct => _mediator.NotifyAsync(notification, errorHandler, ct));
        return Task.CompletedTask;
    }

    public Task NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        INotificationErrorHandler errorHandler, CancellationToken cancellation = default) where TNotification : class
    {
        _outbox.Enqueue(ct => _mediator.NotifyAsync(notification, notifier, errorHandler, ct));
        return Task.CompletedTask;
    }

    public Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
        => _mediator.RequestAsync(request, cancellation);

    public Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
        => _mediator.RequestAsync(request, requestor, cancellation);
}