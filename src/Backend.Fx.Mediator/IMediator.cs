using System.Security.Principal;
using JetBrains.Annotations;

namespace Backend.Fx.Mediator;

[PublicAPI]
public interface IMediator
{
    Task NotifyAsync<TNotification>(TNotification notification, CancellationToken cancellation = default) 
        where TNotification : class;
    
    Task NotifyAsync<TNotification>(
        TNotification notification, 
        IIdentity notifier,
        CancellationToken cancellation = default)
        where TNotification : class;
    
    Task NotifyAsync<TNotification>(
        TNotification notification, 
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default)
        where TNotification : class;
    
    Task NotifyAsync<TNotification>(
        TNotification notification, 
        IIdentity notifier,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default)
        where TNotification : class;
    
    Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class;

    Task<TResponse> RequestAsync<TResponse>(
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class;
}