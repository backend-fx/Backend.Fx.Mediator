using System.Security.Principal;
using Backend.Fx.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator;

[PublicAPI]
public static class BackendFxApplicationMediatorExtensions
{
    /// <summary>
    /// Raises a notification immediately in a <b>separate invocation</b>. This is bypassing any possibly configured outbox!
    /// </summary>
    public static async ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification) where TNotification : class
    {
        await application.Invoker.InvokeAsync(
            (sp, ct) => sp.GetRequiredService<IMediator>().NotifyAsync(notification, ct));
    }
    
    /// <summary>
    /// Raises a notification immediately in a <b>separate invocation</b>. This is bypassing any possibly configured outbox!
    /// </summary>
    public static async ValueTask NotifyAsync<TNotification>(
        this IBackendFxApplication application,
        TNotification notification,
        IIdentity notifier) where TNotification : class
    {
        await application.Invoker.InvokeAsync(
            (sp, ct) => sp.GetRequiredService<IMediator>().NotifyAsync(notification, notifier, ct));
    }
    
    /// <summary>
    /// Executes a request immediately.
    /// </summary>
    public static async ValueTask<TResponse> RequestAsync<TResponse>(
        this IBackendFxApplication application,
        IRequest<TResponse> request,
        CancellationToken cancellation = default) where TResponse : class
    {
        TResponse response = null!;
        
        await application.Invoker.InvokeAsync(
            async (sp, ct) =>
                response = await sp.GetRequiredService<IMediator>().RequestAsync(request, ct),
            null, cancellation);
        
        return response;
    }

    /// <summary>
    /// Executes a request immediately.
    /// </summary>
    public static async ValueTask<TResponse> RequestAsync<TResponse>(
        this IBackendFxApplication application,
        IRequest<TResponse> request,
        IIdentity requestor,
        CancellationToken cancellation = default) where TResponse : class
    {
        TResponse response = null!;
        
        await application.Invoker.InvokeAsync(
            async (sp, ct) =>
                response = await sp.GetRequiredService<IMediator>().RequestAsync(request, requestor, ct),
            null, cancellation);
        
        return response;
    }
}