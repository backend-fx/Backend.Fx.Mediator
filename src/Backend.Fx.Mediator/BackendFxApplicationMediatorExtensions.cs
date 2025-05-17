using System.Security.Principal;
using Backend.Fx.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator;

[PublicAPI]
public static class BackendFxApplicationMediatorExtensions
{
    /// <summary>
    /// Notifies all subscribers immediately.
    /// </summary>
    public static ValueTask NotifyAsync<TNotification>(this IBackendFxApplication application, TNotification notification, CancellationToken cancellation = default) 
        where TNotification : class
        => application.GetMediator().NotifyAsync(notification, cancellation);
    
    /// <summary>
    /// Notifies all subscribers immediately.
    /// </summary>
    public static ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification, 
        IIdentity notifier,
        CancellationToken cancellation = default)
        where TNotification : class
        => application.GetMediator().NotifyAsync(notification, notifier, cancellation);
    
    /// <summary>
    /// Notifies all subscribers immediately.
    /// </summary>
    public static ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification, 
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default)
        where TNotification : class
        => application.GetMediator().NotifyAsync(notification, errorHandler, cancellation);
    
    /// <summary>
    /// Notifies all subscribers immediately.
    /// </summary>
    public static ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification, 
        IIdentity notifier,
        INotificationErrorHandler errorHandler,
        CancellationToken cancellation = default)
        where TNotification : class
        => application.GetMediator().NotifyAsync(notification, notifier, errorHandler, cancellation);
    
    /// <summary>
    /// Executes a request immediately.
    /// </summary>
    public static ValueTask<TResponse> RequestAsync<TResponse>(
        this IBackendFxApplication application,
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
        => application.GetMediator().RequestAsync(request, cancellation);

    /// <summary>
    /// Executes a request immediately.
    /// </summary>
    public static ValueTask<TResponse> RequestAsync<TResponse>(
        this IBackendFxApplication application,
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
        => application.GetMediator().RequestAsync(request, requestor, cancellation);
    
    private static IRootMediator GetMediator(this IBackendFxApplication application)
        => application
            .CompositionRoot
            .ServiceProvider
            .GetRequiredService<IRootMediator>();
}