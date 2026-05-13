using System.Security.Principal;
using Backend.Fx.Execution;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Backend.Fx.Mediator;

[PublicAPI]
public static class BackendFxApplicationMediatorExtensions
{
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