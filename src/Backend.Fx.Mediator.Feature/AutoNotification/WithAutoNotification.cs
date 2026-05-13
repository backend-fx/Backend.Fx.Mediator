using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature.AutoNotification;

public class WithAutoNotification : IMediator
{
    private IMediator _mediator;

    public WithAutoNotification(IMediator mediator)
    {
        _mediator = mediator;
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default) where TNotification : class
    {
        return _mediator.NotifyAsync(notification, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _mediator.NotifyAsync(notification, notifier, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _mediator.NotifyAsync(notification, errorHandler, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        INotificationErrorHandler errorHandler, CancellationToken cancellation = default) where TNotification : class
    {
        return _mediator.NotifyAsync(notification, notifier, errorHandler, cancellation);
    }

    public async ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default) where TResponse : class
    {
        var response = await _mediator.RequestAsync(request, cancellation);
        _ = _mediator.NotifyAsync(response, cancellation);
        return response;
    }

    public async ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, IIdentity requestor, CancellationToken cancellation = default) where TResponse : class
    {
        var response = await _mediator.RequestAsync(request, requestor, cancellation);
        _ = _mediator.NotifyAsync(response, requestor, cancellation);
        return response;
    }
}