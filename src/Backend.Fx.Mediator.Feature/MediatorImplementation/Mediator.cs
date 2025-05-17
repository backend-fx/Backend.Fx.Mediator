using System.Security.Principal;

namespace Backend.Fx.Mediator.Feature.MediatorImplementation;

internal sealed class Mediator : IMediator
{
    private readonly IRootMediator _rootMediator;
    
    public Mediator(IRootMediator rootMediator)
    {
        _rootMediator = rootMediator;
    }


    public ValueTask NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, notifier, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, errorHandler, cancellation);
    }

    public ValueTask NotifyAsync<TNotification>(TNotification notification, IIdentity notifier,
        INotificationErrorHandler errorHandler, CancellationToken cancellation = default) where TNotification : class
    {
        return _rootMediator.NotifyAsync(notification, notifier, errorHandler, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, CancellationToken cancellation = default) where TResponse : class
    {
        return _rootMediator.RequestAsync(request, cancellation);
    }

    public ValueTask<TResponse> RequestAsync<TResponse>(IRequest<TResponse> request, IIdentity requestor, CancellationToken cancellation = default) where TResponse : class
    {
        return _rootMediator.RequestAsync(request, requestor, cancellation);
    }
}