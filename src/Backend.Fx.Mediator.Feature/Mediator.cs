using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature;

internal sealed class Mediator : IMediator
{
    private readonly IApplicationMediator _applicationMediator;
    
    public Mediator(IApplicationMediator applicationMediator)
    {
        _applicationMediator = applicationMediator;
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default)
        where TNotification : class
    {
        return _applicationMediator.NotifyAsync(notification, null, null, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _applicationMediator.NotifyAsync(notification, notifier, null, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(
        TNotification notification,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _applicationMediator.NotifyAsync(notification, null, errorHandler, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(
        TNotification notification,
        IIdentity notifier,
        INotificationErrorHandler errorHandler, CancellationToken cancellation = default) where TNotification : class
    {
        return _applicationMediator.NotifyAsync(notification, notifier, errorHandler, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
    {
        return _applicationMediator.RequestAsync(request, null, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
    {
        return _applicationMediator.RequestAsync(request, requestor, cancellation);
    }
}